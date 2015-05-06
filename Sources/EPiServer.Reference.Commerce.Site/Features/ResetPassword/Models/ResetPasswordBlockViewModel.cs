using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Blocks;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models
{
    public class ResetPasswordBlockViewModel
    {
        public ResetPasswordBlock CurrentBlock { get; set; }
        public ResetPasswordBlockFormModel FormModel { get; set; }
        public PageReference CurrentPageLink { get; set; }
        public bool Success { get; set; }
    }
}