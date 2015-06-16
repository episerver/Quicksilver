using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Models
{
    public class HeaderViewModel
    {
        public ContentReference CurrentContentLink { get; set; }
        public StartPage StartPage { get; set; }
        public string UserDisplayName { get; set; }
    }
}