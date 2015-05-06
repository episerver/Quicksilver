using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.BaseLibrary;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market
{
    public class LanguageService
    {
        private const string LanguageCookie = "Language";
        private readonly ICurrentMarket _currentMarket;
        private readonly CookieService _cookieService;

        public LanguageService(ICurrentMarket currentMarket, CookieService cookieService)
        {
            _currentMarket = currentMarket;
            _cookieService = cookieService;
        }

        public IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            return CurrentMarket.Languages;
        }
        
        public CultureInfo GetCurrentLanguage()
        {
            return TryGetLanguage(_cookieService.Get(LanguageCookie)) ?? CurrentMarket.DefaultLanguage;
        }

        public bool SetCurrentLanguage(string language)
        {
            var culture = TryGetLanguage(language);
            if (culture == null)
                return false;
            Context.Current["EPiServer:ContentLanguage"] = culture;
            _cookieService.Set(LanguageCookie, language);
            return true;
        }

        private CultureInfo TryGetLanguage(string language)
        {
            if (language == null)
                return null;
            try
            {
                var culture = CultureInfo.GetCultureInfo(language);
                return GetAvailableLanguages().FirstOrDefault(c => c.Name == culture.Name);
            }
            catch (CultureNotFoundException)
            {
                return null;
            }
        }

        private IMarket CurrentMarket
        {
            get { return _currentMarket.GetCurrentMarket(); }
        }
    }
}