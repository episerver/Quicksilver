using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels
{
    public class ResetPasswordMailModel
    {
        public ResetPasswordMailPage CurrentPage { get; set; }

        public string CallbackUrl { get; set; }
    }
}