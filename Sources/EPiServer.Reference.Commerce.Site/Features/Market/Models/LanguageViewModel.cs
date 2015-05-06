using System.Collections.Generic;
using System.Globalization;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Models
{
    public class LanguageViewModel
    {
        public IEnumerable<CultureInfo> Languages { get; set; }
        public CultureInfo CurrentLanguage { get; set; }
    }
}