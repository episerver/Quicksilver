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
                throw new ArgumentNullException("email");
            }

            CustomerContact contact = null;
            SiteUser user = GetUser(email);

            if (user != null)
            {
                contact = _customerContext.GetContactById(new Guid(user.Id));
            }

            return contact;
        }

        public virtual CustomerContact GetCustomerContact(PrimaryKeyId primaryKeyId)
        {
            return _customerContext.GetContactById(primaryKeyId);
        }

        public virtual PrimaryKeyId? GetCustomerContactPrimaryKeyId(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            var contact = GetCustomerContact(email);
            if(contact == null)
            {
                return null;
            }
            return contact.PrimaryKeyId;
        }

        public virtual SiteUser GetUser(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            SiteUser user = _userManager.FindByEmail(email);

            return user;
        }

        public virtual async Task<SiteUser> GetUserAsync(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            return await _userManager.FindByNameAsync(email);
        }

        public virtual async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _authenticationManager.GetExternalLoginInfoAsync();
        }

        public virtual async Task<ContactIdentityResult> RegisterAccount(SiteUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            ContactIdentityResult contactResult = null;
            IdentityResult result = null;
            CustomerContact contact = null;

            if (String.IsNullOrEmpty(user.Password))
            {
                throw new MissingFieldException("Password");
            }

            if (String.IsNullOrEmpty(user.Email))
            {
                throw new MissingFieldException("Email");
            }

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

            contactResult = new ContactIdentityResult(result, contact);

            return contactResult;
        }

        private CustomerContact CreateCustomerContact(SiteUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            CustomerContact contact = CustomerContact.CreateInstance();

            // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
            // Send an email with this link
            // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            if (!String.IsNullOrEmpty(user.FirstName) || !String.IsNullOrEmpty(user.LastName))
            {
                contact.FullName = String.Format("{0} {1}", user.FirstName, user.LastName);
            }

            contact.PrimaryKeyId = new PrimaryKeyId(new Guid(user.Id));
            contact.FirstName = user.FirstName;
            contact.LastName = user.LastName;
            contact.Email = user.Email;
            contact.UserId = "String:" + user.Email; // The UserId needs to be set in the format "String:{email}". Else a duplicate CustomerContact will be created later on.
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

        public void SignOut()
        {
            _authenticationManager.SignOut();
        }

        public void Dispose()
        {
            if (_userManager != null)
            {
                _userManager.Dispose();
            }

            if (_signInManager != null)
            {
                _signInManager.Dispose();
            }
        }
    }
}