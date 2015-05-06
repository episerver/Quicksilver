using System.ComponentModel.DataAnnotations;
using EPiServer.Framework.Localization;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class LocalizedRegularExpressionAttribute : RegularExpressionAttribute
    {
        private readonly string _name;

        public LocalizedRegularExpressionAttribute(string pattern, string name)
            : base(pattern)
        {
            _name = name;
        }

        public override string FormatErrorMessage(string name)
        {
            ErrorMessage = LocalizationService.Current.GetString(_name);
            return base.FormatErrorMessage(name);
        }
    }
}