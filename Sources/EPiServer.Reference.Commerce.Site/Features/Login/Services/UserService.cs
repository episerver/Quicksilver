using System.Linq;
using System.Web.Security;
using Mediachase.Commerce.Customers;
using EPiServer.Reference.Commerce.Site.Features.Login.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System;
using EPiServer.Framework.Localization;
using Microsoft.AspNet.Identity.Owin;
using EPiServer.Reference.Commerce.Site.Features.Registration.Models;
using Mediachase.Commerce.Orders;
using Microsoft.Owin.Security;
using Microsoft.Owin;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Services
{
    /// <summary>
    /// Service class for working with user accounts.
    /// </summary>
    public class UserService
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private IAuthenticationManager _authenticationManager;

        /// <summary>
        /// Returns a new instance of a UserService. It will use the current HTTP context to
        /// retrieve the ApplicationUserManager, IAuthenticationManager and the ApplicationSignInManager.
        /// </summary>
        public UserService()
            : this(HttpContext.Current.GetOwinContext())
        {
        }

        /// <summary>
        /// Returns a new instance of a UserService. It will use an provided IOwinContext to
        /// retrieve the ApplicationUserManager, IAuthenticationManager and the ApplicationSignInManager.
        /// </summary>
        public UserService(IOwinContext context)
            : this(
            context.GetUserManager<ApplicationUserManager>(),
            context.Get<ApplicationSignInManager>(),
            context.Authentication)
        {
        }

        /// <summary>
        /// Returns a new instance of a UserService. The ApplicationUserManager, IAuthenticationManager and the ApplicationSignInManager
        /// needs to be proviced by the caller.
        /// </summary>
        /// <param name="userManager">The ApplicationUserManager for working with user accounts.</param>
        /// <param name="signInManager">The ApplicationSignInManager for signing in an existing user.</param>
        public UserService(ApplicationUserManager userManager, ApplicationSignInManager signInManager, IAuthenticationManager authenticationManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authenticationManager = authenticationManager;
        }

        /// <summary>
        /// Gets a CustomerContact based on her e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address belonging to the CustomerContact.</param>
        /// <returns>Returns the CustomerContact having the same e-mail address as the one provided. If no contact exist
        /// then null is returned instead.</returns>
        public CustomerContact GetCustomerContact(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            CustomerContact contact = null;
            ApplicationUser user = GetUser(email);

            if (user != null)
            {
                contact = CustomerContext.Current.GetContactById(new Guid(user.Id));
            }

            return contact;
        }

        /// <summary>
        /// Gets an existing ApplicationUser based the user's e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address belonging to the ApplicationUser.</param>
        /// <returns>Returns the ApplicationUser having the same e-mail address as the one provided. If no user exist
        /// then null is returned instead.</returns>
        public ApplicationUser GetUser(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            ApplicationUser user = _userManager.FindByEmail(email);

            return user;
        }

        /// <summary>
        /// Gets an existing ApplicationUser asynchronously based the user's e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address belonging to the ApplicationUser.</param>
        /// <returns>Returns the ApplicationUser having the same e-mail address as the one provided. If no user exist
        /// then null is returned instead.</returns>
        public async Task<ApplicationUser> GetUserAsync(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            return await _userManager.FindByNameAsync(email);
        }

        /// <summary>
        /// Gets user login information retreived from an external login provider.
        /// </summary>
        /// <returns>Gets an ExternalLoginInfo object for the current user.</returns>
        public async Task<ExternalLoginInfo> GetExternalLoginInfoAsync()
        {
            return await _authenticationManager.GetExternalLoginInfoAsync();
        }

        /// <summary>
        /// Creates a new user account and adds an associated CustomerContact to it.
        /// </summary>
        /// <param name="user">The ApplicationUser to create.</param>
        /// <returns>Returns a ContactIdentityResult holding both the result of creating the user account as well as the
        /// CustomerContact if such was successfully stored.</returns>
        public async Task<ContactIdentityResult> RegisterAccount(ApplicationUser user)
        {
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
                result = new IdentityResult(new string[] { LocalizationService.Current.GetString("/Registration/Form/Error/UsedEmail") });
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

        /// <summary>
        /// Creates a new CustomerContact based on an ApplicationUser and saves it to the database.
        /// </summary>
        /// <param name="user">The user account that should be the base for the new CustomerContact.</param>
        /// <returns>Returns the created CustomerContact.</returns>
        private CustomerContact CreateCustomerContact(ApplicationUser user)
        {
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

            contact.PrimaryKeyId = new Mediachase.BusinessFoundation.Data.PrimaryKeyId(new Guid(user.Id));
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

            // Once the contact has been saved we can look for any existing addresses.
            CustomerAddress defaultAddress = contact.ContactAddresses.FirstOrDefault();
            if (defaultAddress != null)
            {
                // If an addresses was found, it will be used as default for shipping and billing.
                contact.PreferredShippingAddress = defaultAddress;
                contact.PreferredBillingAddress = defaultAddress;

                // Save the address preferrences also.
                contact.SaveChanges();
            }


            return contact;
        }

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        public void SignOut()
        {
            _authenticationManager.SignOut();
        }
    }
}