using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Data.Dynamic;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Customers;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Services
{
    class OptinTokenData : IDynamicData
    {
        public Data.Identity Id { get; set; }
        public string UserId { get; set; }
        public string OptinConfirmationToken { get; set; }
        public DateTime Created { get; set; }
    }

    public class OptinService : IDisposable
    {
        const string MarketingEmailOptinPurpose = "Marketing email opt-in confirmation";
        readonly DynamicDataStoreFactory _storeFactory;
        readonly ApplicationUserManager<SiteUser> _userManager;
        readonly CustomerContextFacade _customerContext;

        public OptinService(DynamicDataStoreFactory storeFactory, ApplicationUserManager<SiteUser> userManager, CustomerContextFacade customerContext)
        {
            _storeFactory = storeFactory;
            _userManager = userManager;
            _customerContext = customerContext;
        }

        DynamicDataStore TokenStore => _storeFactory.GetStore(typeof(OptinTokenData)) ??
                                                _storeFactory.CreateStore(typeof(OptinTokenData));

        public void Dispose()
        {
            _userManager?.Dispose();
        }

        public virtual async Task<string> CreateOptinTokenData(string userId)
        {
            var token = await _userManager.GenerateUserTokenAsync(MarketingEmailOptinPurpose, userId);
            var tokenData = new OptinTokenData
            {
                UserId = userId,
                OptinConfirmationToken = token,
                Created = DateTime.Now
            };

            TokenStore.Save(tokenData);
            return tokenData.OptinConfirmationToken;
        }

        public async Task<bool> ConfirmOptinToken(string userId, string token)
        {
            var userOptinTokenData = TokenStore.Find<OptinTokenData>("UserId", userId).FirstOrDefault();
            if (userOptinTokenData == null)
            {
                return false;
            }

            var confirmResult = await _userManager.VerifyUserTokenAsync(userId, MarketingEmailOptinPurpose, token);
            if (!confirmResult)
            {
                return false;
            }

            // Update consent data to CustomerContact
            var user = _userManager.FindById(userId);
            var contact = _customerContext.GetContactByUsername(user.UserName);
            UpdateOptin(contact, true);

            // Delete token data from DDS
            TokenStore.Delete(userOptinTokenData.Id);
            return true;
        }

        public ConsentData GetCurrentContactConsentData()
        {
            var currentContact = GetCurrentContact();
            return new ConsentData
            {
                AcceptMarketingEmail = currentContact.AcceptMarketingEmail,
                ConsentUpdated = currentContact.ConsentUpdated
            };
        }

        public void UpdateOptinForCurrentContact(bool acceptMarketingEmail)
        {
            var currentContact = GetCurrentContact();
            UpdateOptin(currentContact, acceptMarketingEmail);
        }

        private void UpdateOptin(CustomerContact contact, bool acceptMarketingEmail)
        {
            if (contact.AcceptMarketingEmail != acceptMarketingEmail)
            {
                contact.AcceptMarketingEmail = acceptMarketingEmail;
                contact.ConsentUpdated = DateTime.Now;
                contact.SaveChanges();
            }
        }

        private CustomerContact GetCurrentContact()
        {
            return _customerContext.CurrentContact.CurrentContact;
        }
    }
}