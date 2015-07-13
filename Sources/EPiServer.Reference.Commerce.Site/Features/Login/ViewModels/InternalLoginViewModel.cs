using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class InternalLoginViewModel : LoginViewModelBase
    {

        [LocalizedDisplay("/Login/Form/Label/Email")]
        [LocalizedRequired("/Login/Form/Empty/Email")]
        [LocalizedEmail("/Login/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        public PageReference ResetPasswordPage { get; set; }

    }
}