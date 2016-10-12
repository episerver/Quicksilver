using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Start.Models
{
    public class StartPageViewModel
    {
        public StartPage StartPage { get; set; }

        public IEnumerable<PromotionViewModel> Promotions { get; set; }
    }
}