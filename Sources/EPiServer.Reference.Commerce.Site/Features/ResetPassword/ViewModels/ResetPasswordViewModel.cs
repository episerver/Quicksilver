using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels
{
    public class ResetPasswordViewModel
    {
        [LocalizedDisplay("/ResetPassword/Form/Label/Email")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Email")]
        [LocalizedEmail("/ResetPassword/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        [LocalizedDisplay("/ResetPassword/Form/Label/Password")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password")]
        public string Password { get; set; }

        public string Code { get; set; }

        [LocalizedDisplay("/ResetPassword/Form/Label/Password2")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password2")]
        [LocalizedCompare("Password", "/ResetPassword/Form/Error/PasswordMatch")]
        [LocalizedStringLength("/ResetPassword/Form/Error/PasswordLength2", 5, 100)]
        public string Password2 { get; set; }

    }
}