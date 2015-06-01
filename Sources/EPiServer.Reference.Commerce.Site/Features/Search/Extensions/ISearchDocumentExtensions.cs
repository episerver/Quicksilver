using Lucene.Net.Util;
using Mediachase.Search;
using System;

namespace EPiServer.Reference.Commerce.Site.Features.Search.Extensions
{
    public static class ISearchDocumentExtensions
    {
        public static bool _isLuceneProvider;

        static ISearchDocumentExtensions()
        {
            _isLuceneProvider = false;
            var element = SearchConfiguration.Instance.SearchProviders;
            if (element.Providers == null ||
                String.IsNullOrEmpty(element.DefaultProvider) ||
                String.IsNullOrEmpty(element.Providers[element.DefaultProvider].Type))
            {
                return;
            }

            var providerType = Type.GetType(element.Providers[element.DefaultProvider].Type);
            var baseType = Type.GetType("Mediachase.Search.Providers.Lucene.LuceneSearchProvider, Mediachase.Search.LuceneSearchProvider");
            if (providerType == null || baseType == null)
            {
                return;
            }
            if (providerType == baseType || providerType.IsSubclassOf(baseType))
            {
                _isLuceneProvider = true;
            }
        }

        public static string GetString(this ISearchDocument document, string name)
        {
            return document[name] != null ? document[name].Value.ToString() : "";
        }

        public static decimal GetDecimal(this ISearchDocument document, string name)
        {
            if (document[name] == null)
            {
                return 0m;
            }
            return _isLuceneProvider
                        ? Convert.ToDecimal(NumericUtils.PrefixCodedToLong(document[name].Value.ToString()) / 10000m)
                        : decimal.Parse(document[name].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }
    }
}