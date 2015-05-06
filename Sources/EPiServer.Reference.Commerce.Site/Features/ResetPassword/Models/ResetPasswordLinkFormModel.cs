using System.ComponentModel.DataAnnotations;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models
{
    public class ResetPasswordLinkFormModel
    {
        [Required]
        public string Hash { get; set; }

        [LocalizedDisplay("/ResetPassword/Form/Label/Password")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password")]
        [LocalizedStringLength("/ResetPassword/Form/Error/PasswordLength2", 5, 100)]
        public string Password { get; set; }

        [LocalizedDisplay("/ResetPassword/Form/Label/Password2")]
        [LocalizedRequired("/ResetPassword/Form/Empty/Password2")]
        [LocalizedCompare("Password", "/ResetPassword/Form/Error/PasswordMatch")]
        [LocalizedStringLength("/ResetPassword/Form/Error/PasswordLength2", 5, 100)]
        public string Password2 { get; set; }
    }
}