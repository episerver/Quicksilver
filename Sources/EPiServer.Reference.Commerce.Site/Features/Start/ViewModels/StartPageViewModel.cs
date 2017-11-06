using EPiServer.Core;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Tracking.Commerce;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Start.ViewModels
{
    public class StartPageViewModel
    {
        public StartPage StartPage { get; set; }
        public IEnumerable<PromotionViewModel> Promotions { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
    }
}