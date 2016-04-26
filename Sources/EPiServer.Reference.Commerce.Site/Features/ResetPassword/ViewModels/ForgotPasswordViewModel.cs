using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

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