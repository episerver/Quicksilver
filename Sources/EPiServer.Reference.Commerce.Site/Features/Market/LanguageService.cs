using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Globalization;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market
{
    public class LanguageService : IUpdateCurrentLanguage
    {
        private const string LanguageCookie = "Language";
        private readonly ICurrentMarket _currentMarket;
        private readonly CookieService _cookieService;
        private readonly IUpdateCurrentLanguage _defaultUpdateCurrentLanguage;
        private readonly RequestContext _requestContext;

        public LanguageService(ICurrentMarket currentMarket, CookieService cookieService, IUpdateCurrentLanguage defaultUpdateCurrentLanguage, RequestContext requestContext)
        {
            _currentMarket = currentMarket;
            _cookieService = cookieService;
            _defaultUpdateCurrentLanguage = defaultUpdateCurrentLanguage;
            _requestContext = requestContext;
        }

        public IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            return CurrentMarket.Languages;
        }
        
        public CultureInfo GetCurrentLanguage()
        {
            return TryGetLanguage(_cookieService.Get(LanguageCookie)) ?? CurrentMarket.DefaultLanguage;
        }

        public virtual bool SetCurrentLanguage(string language)
        {
            var culture = TryGetLanguage(language);
            if (culture == null)
            {
                return false;
            }

            _defaultUpdateCurrentLanguage.UpdateLanguage(language);
            _cookieService.Set(LanguageCookie, language);
            return true;
        }

        private CultureInfo TryGetLanguage(string language)
        {
            if (language == null)
            {
                return null;
            }

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

        public void UpdateLanguage(string languageId)
        {

            if (_requestContext.HttpContext != null && _requestContext.HttpContext.Request.Url != null && _requestContext.HttpContext.Request.Url.AbsolutePath == "/")
            {
                var languageCookie = _cookieService.Get(LanguageCookie);
                if (languageCookie != null)
                {
                    _defaultUpdateCurrentLanguage.UpdateLanguage(languageCookie);
                    return;
                }

                var currentMarket = _currentMarket.GetCurrentMarket();
                if (currentMarket != null && currentMarket.DefaultLanguage != null)
                {
                    _defaultUpdateCurrentLanguage.UpdateLanguage(currentMarket.DefaultLanguage.Name);
                    return;
                }
            }

            _defaultUpdateCurrentLanguage.UpdateLanguage(languageId);
        }

        public void UpdateReplacementLanguage(IContent currentContent, string requestedLanguage)
        {
            _defaultUpdateCurrentLanguage.UpdateReplacementLanguage(currentContent, requestedLanguage);
        }
    }
}