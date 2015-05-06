using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models
{
    public class ResetPasswordLinkViewModel
    {
        public PasswordResetLinkPage CurrentBlock { get; set; }
        public ResetPasswordLinkFormModel FormModel { get; set; }
        public PageReference CurrentPageLink { get; set; }

        public bool Success { get; set; }

        public bool InvalidHash { get; set; }
    }
}