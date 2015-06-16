using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels
{
    public class ResetPasswordViewModel
    {

        /// <summary>
        /// Gets or sets the user's e-mail address.
        /// </summary>
        [LocalizedDisplay("/ResetPassword/Form/Label/Email")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Email")]
        [LocalizedEmail("/ResetPassword/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's current password.
        /// </summary>
        [LocalizedDisplay("/ResetPassword/Form/Label/Password")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation code used when resetting
        /// a user's password.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the second entered password.
        /// </summary>
        [LocalizedDisplay("/ResetPassword/Form/Label/Password2")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password2")]
        [LocalizedCompare("Password", "/ResetPassword/Form/Error/PasswordMatch")]
        [LocalizedStringLength("/ResetPassword/Form/Error/PasswordLength2", 5, 100)]
        public string Password2 { get; set; }

    }
}