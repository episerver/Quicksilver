using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    /// <summary>
    /// View model used for authenticating user accounts created locally and stored in the
    /// site's own database.
    /// </summary>
    public class InternalLoginViewModel : LoginViewModelBase
    {

        /// <summary>
        /// Gets or sets the user's e-mail address.
        /// </summary>
        [LocalizedDisplay("/Login/Form/Label/Email")]
        [LocalizedRequired("/Login/Form/Empty/Email")]
        [LocalizedEmail("/Login/Form/Error/InvalidEmail")]
        public string Email { get; set; }

    }
}