using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        private readonly string _translationPath;

        public LocalizedRequiredAttribute(string translationPath)
        {
            _translationPath = translationPath;
        }

        public override string FormatErrorMessage(string name)
        {
            ErrorMessage = LocalizationService.Current.GetString(_translationPath);
            return base.FormatErrorMessage(name);
        }
    }
}