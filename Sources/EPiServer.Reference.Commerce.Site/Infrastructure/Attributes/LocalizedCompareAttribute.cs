using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class LocalizedCompareAttribute : CompareAttribute
    {
        private readonly string _translationPath;

        public LocalizedCompareAttribute(string otherProperty, string translationPath)
            : base(otherProperty)
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