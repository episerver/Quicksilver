using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models
{
    public class ResetPasswordBlockFormModel
    {
        [LocalizedDisplay("/Login/Form/Label/Email")]
        [LocalizedRequired("/Login/Form/Empty/Email")]
        [LocalizedEmail("/Login/Form/Error/InvalidEmail")]
        public string Email { get; set; }
    }
}