using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.BusinessFoundation.Data;
using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Services
{
    public class UserService : IDisposable
    {
        private readonly ApplicationUserManager<SiteUser> _userManager;
        private readonly ApplicationSignInManager<SiteUser> _signInManager;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly LocalizationService _localizationService;
        private readonly CustomerContextFacade _customerContext;
        
        public UserService(ApplicationUserManager<SiteUser> userManager, 
            ApplicationSignInManager<SiteUser> signInManager, 
            IAuthenticationManager authenticationManager,
            LocalizationService localizationService,
            CustomerContextFacade customerContextFacade)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authenticationManager = authenticationManager;
            _localizationService = localizationService;
            _customerContext = customerContextFacade;
        }

        public virtual CustomerContact GetCustomerContact(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }
            
            CustomerContact contact = null;
            var user = GetUser(email);

            if (user != null)
            {
                contact = _customerContext.GetContactById(new Guid(user.Id));
            }

            return contact;
        }

        public virtual SiteUser GetUser(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            SiteUser user = _userManager.FindByEmail(email);

            return user;
        }

        public virtual async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _authenticationManager.GetExternalLoginInfoAsync();
        }

        public virtual async Task<ContactIdentityResult> RegisterAccount(SiteUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(user.Password))
            {
                throw new MissingFieldException("Password");
            }

            if (String.IsNullOrEmpty(user.Email))
            {
                throw new MissingFieldException("Email");
            }

            IdentityResult result;
            CustomerContact contact = null;

            if (_userManager.FindByEmail(user.Email) != null)
            {
                result = new IdentityResult(_localizationService.GetString("/Registration/Form/Error/UsedEmail"));
            }
            else
            {
                result = await _userManager.CreateAsync(user, user.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    contact = CreateCustomerContact(user);
                }
            }

            var contactResult = new ContactIdentityResult(result, contact);

            return contactResult;
        }

        public CustomerContact CreateCustomerContact(SiteUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            CustomerContact contact = _customerContext.GetContactByUsername(user.UserName);
            if (contact == null)
            {
                contact = CustomerContact.CreateInstance();
                contact.PrimaryKeyId = new PrimaryKeyId(new Guid(user.Id));
                contact.UserId = "String:" + user.Email; // The UserId needs to be set in the format "String:{email}". Else a duplicate CustomerContact will be created later on.
            }

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
            if (!String.IsNullOrEmpty(user.FirstName) || !String.IsNullOrEmpty(user.LastName))
            {
                contact.FullName = $"{user.FirstName} {user.LastName}";
            }

            contact.FirstName = user.FirstName;
            contact.LastName = user.LastName;
            contact.Email = user.Email;
            contact.RegistrationSource = user.RegistrationSource;

            if (user.Addresses != null)
            {
                foreach (var address in user.Addresses)
                {
                    contact.AddContactAddress(address);
                }
            }

            // The contact, or more likely its related addresses, must be saved to the database before we can set the preferred
            // shipping and billing addresses. Using an address id before its saved will throw an exception because its value
            // will still be null.
            contact.SaveChanges();

            SetPreferredAddresses(contact);

            return contact;
        }

        public void Dispose()
        {
            _userManager?.Dispose();

            _signInManager?.Dispose();
        }

        public void SignOut()
        {
            _authenticationManager.SignOut();
        }

        private void SetPreferredAddresses(CustomerContact contact)
        {
            var changed = false;

            var publicAddress = contact.ContactAddresses.FirstOrDefault(a => a.AddressType == CustomerAddressTypeEnum.Public);
            var preferredBillingAddress = contact.ContactAddresses.FirstOrDefault(a => a.AddressType == CustomerAddressTypeEnum.Billing);
            var preferredShippingAddress = contact.ContactAddresses.FirstOrDefault(a => a.AddressType == CustomerAddressTypeEnum.Shipping);

            if (publicAddress != null)
            {
                contact.PreferredShippingAddress = contact.PreferredBillingAddress = publicAddress;
                changed = true;
            }

            if (preferredBillingAddress != null)
            {
                contact.PreferredBillingAddress = preferredBillingAddress;
                changed = true;
            }

            if (preferredShippingAddress != null)
            {
                contact.PreferredShippingAddress = preferredShippingAddress;
                changed = true;
            }

            if (changed)
            {
                contact.SaveChanges();
            }
        }
    }
}