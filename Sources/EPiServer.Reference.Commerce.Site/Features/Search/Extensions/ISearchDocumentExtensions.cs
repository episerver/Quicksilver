using Mediachase.Search;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Extensions
{
    public static class ISearchDocumentExtensions
    {
        public static string GetString(this ISearchDocument document, string name)
        {
            return document[name] != null ? document[name].Value.ToString() : "";
        }

        public static decimal GetDecimal(this ISearchDocument document, string name)
        {
            return document[name] != null ? decimal.Parse(document[name].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat) : 0m;
        }
    }
}