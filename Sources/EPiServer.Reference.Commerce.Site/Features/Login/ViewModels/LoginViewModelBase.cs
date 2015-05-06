using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    /// <summary>
    /// Base class for all login view model.
    /// </summary>
    public abstract class LoginViewModelBase
    {

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [LocalizedDisplay("/Login/Form/Label/Password")]
        [LocalizedRequired("/Login/Form/Empty/Password")]
        public string Password { get; set; }

        /// <summary>
        /// The URL used for re-directing the user to the previous view.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// Gets or sets whether the user wants to be remembered when re-visiting the site.
        /// </summary>
        [LocalizedDisplay("/Login/Form/Label/RememberMe")]
        public bool RememberMe { get; set; }

    }
}