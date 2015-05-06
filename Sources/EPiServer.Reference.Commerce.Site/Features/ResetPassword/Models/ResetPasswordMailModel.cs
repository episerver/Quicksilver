using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models
{
    public class ResetPasswordMailModel
    {
        public ResetPasswordMailPage CurrentPage { get; set; }
        public string Hash { get; set; }

        public StartPage StartPage { get; set; }
    }
}