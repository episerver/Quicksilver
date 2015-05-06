using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Security;
using EPiServer.Reference.Commerce.Site.Features.Login;
using Mediachase.Commerce.Customers;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.ServiceLocation;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword
{
    [ServiceConfiguration(typeof(IPasswordHashService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class PasswordHashService : IPasswordHashService
    {
        private readonly UserService _userService;

        public PasswordHashService(UserService userService)
        {
            _userService = userService;
        }

        public byte[] CreateHash(string email)
        {
            CustomerContact contact = _userService.GetCustomerContact(email);

            if (contact == null || !contact.PrimaryKeyId.HasValue)
            { 
                return null;
            }

            var id = contact.PrimaryKeyId.Value;
            int validTimeConfig = Convert.ToInt32(ConfigurationManager.AppSettings.Get("PasswordResetValidMinutes"));
            DateTime validTime = DateTime.UtcNow.AddMinutes(validTimeConfig == 0 ? 60 : validTimeConfig);
            string infoString = string.Format("{0};{1};{2}", id, email, validTime.Ticks);
            byte[] stream = Encoding.UTF8.GetBytes(infoString);

            return MachineKey.Protect(stream, "Reset password");
        }

        public CustomerContact DecodeValidateHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
            { 
                return null;
            }

            var urlDecodedHash = HttpServerUtility.UrlTokenDecode(hash);
            
            if (urlDecodedHash == null)
            {
                return null;
            }

            var decodedValue = MachineKey.Unprotect(urlDecodedHash, "Reset password");
            
            if (decodedValue == null)
            {
                return null;
            }

            var data = Encoding.UTF8.GetString(decodedValue);
            var s = data.Split(';');
            Guid id;
            long validTime;
            long.TryParse(s[2], out validTime);
            Guid.TryParse(s[0], out id);

            var contact = _userService.GetCustomerContact(s[1]);
            
            if (contact == null || (contact.PrimaryKeyId.HasValue && id != contact.PrimaryKeyId.Value))
            {
                return null;
            }

            return DateTime.UtcNow.Ticks > validTime ? null : contact;
        }
    }
}