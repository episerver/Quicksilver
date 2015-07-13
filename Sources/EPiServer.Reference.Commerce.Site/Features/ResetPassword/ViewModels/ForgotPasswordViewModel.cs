using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public ResetPasswordPage CurrentPage { get; set; }

        [LocalizedDisplay("/ResetPassword/Form/Label/Email")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Email")]
        [LocalizedEmail("/ResetPassword/Form/Error/InvalidEmail")]
        public string Email { get; set; }
    }
}