using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;

namespace EPiServer.Reference.Commerce.Site.Features.Login.ViewModels
{
    public class InternalLoginViewModel : LoginViewModelBase
    {

        [LocalizedDisplay("/Login/Form/Label/Email")]
        [LocalizedRequired("/Login/Form/Empty/Email")]
        [LocalizedEmail("/Login/Form/Error/InvalidEmail")]
        public string Email { get; set; }

        public ContentReference ResetPasswordPage { get; set; }

    }
}