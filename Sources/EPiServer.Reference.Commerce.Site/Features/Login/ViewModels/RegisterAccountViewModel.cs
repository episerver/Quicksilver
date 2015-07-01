using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    /// <summary>
    /// View model used when registering new user accounts.
    /// </summary>
    public class RegisterAccountViewModel 
    {
        public Address Address { get; set; }

        /// <summary>
        /// Gets or sets the E-mail address.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Email")]
        [LocalizedRequired("/Registration/Form/Empty/Email")]
        [LocalizedEmail("/Registration/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the first entered password.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Password")]
        [LocalizedRequired("/Registration/Form/Empty/Password")]
        [LocalizedStringLength("/Registration/Form/Error/PasswordLength2", 5, 100)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the second entered password.
        /// </summary>
        [LocalizedDisplay("/Registration/Form/Label/Password2")]
        [LocalizedRequired("/Registration/Form/Empty/Password2")]
        [LocalizedCompare("Password", "/Registration/Form/Error/PasswordMatch")]
        [LocalizedStringLength("/Registration/Form/Error/PasswordLength2", 5, 100)]
        public string Password2 { get; set; }

        /// <summary>
        /// Gets or sets whether the user wishes to subscribe to newsletters.
        /// </summary>
        public bool Newsletter { get; set; }

        public string ErrorMessage { get; set; }
    }
}