using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Routing;
using EPiServer.Core;
using EPiServer.Globalization;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Market.Services
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

        public virtual IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            return CurrentMarket.Languages;
        }
        
        public virtual CultureInfo GetCurrentLanguage()
        {
            CultureInfo cultureInfo;
            return TryGetLanguage(_cookieService.Get(LanguageCookie), out cultureInfo)
                ? cultureInfo
                : CurrentMarket.DefaultLanguage;
        }

        public virtual bool SetCurrentLanguage(string language)
        {
            CultureInfo cultureInfo;
            if (!TryGetLanguage(language, out cultureInfo))
            {
                return false;
            }

            _defaultUpdateCurrentLanguage.UpdateLanguage(language);
            _cookieService.Set(LanguageCookie, language);
            return true;
        }

        private bool TryGetLanguage(string language, out CultureInfo cultureInfo)
        {
            cultureInfo = null;

            if (language == null)
            {
                return false;
            }

            try
            {
                var culture = CultureInfo.GetCultureInfo(language);
                cultureInfo = GetAvailableLanguages().FirstOrDefault(c => c.Name == culture.Name);
                return cultureInfo != null;
            }
            catch (CultureNotFoundException)
            {
                return false;
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