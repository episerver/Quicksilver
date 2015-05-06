using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    /// <summary>
    /// View model used for authenticating a user against the backend Edit views.
    /// </summary>
    public class BackendLoginViewModel : LoginViewModelBase
    {

        /// <summary>
        /// Gets or sets the user name for the account being authenticated.
        /// </summary>
        [LocalizedDisplay("/Login/Form/Label/Username")]
        [LocalizedRequired("/Login/Form/Empty/Username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the title displayed on the login view.
        /// </summary>
        public string Heading { get; set; }

        /// <summary>
        /// Gets or sets the login message displayed on the login view.
        /// </summary>
        public string LoginMessage { get; set; }
        
    }
}