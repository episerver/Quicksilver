using System.Collections.Generic;
using System.Web.Mvc;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class CurrencyViewModel
    {
        public IEnumerable<SelectListItem> Currencies { get; set; }
        public string CurrencyCode { get; set; }
    }
}