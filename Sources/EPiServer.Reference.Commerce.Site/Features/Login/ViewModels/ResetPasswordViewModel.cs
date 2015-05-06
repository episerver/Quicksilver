using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class ResetPasswordViewModel
    {

        /// <summary>
        /// Gets or sets the user's e-mail address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's current password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the confirmation code used when resetting
        /// a user's password.
        /// </summary>
        public string Code { get; set; }

    }
}