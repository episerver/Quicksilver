using System;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class StringExtensions
    {
        public static bool IsLocalUrl(this string url, HttpRequestBase request)
        {
            Uri absoluteUri;
            return Uri.TryCreate(url, UriKind.Absolute, out absoluteUri) && String.Equals(request.Url.Host, absoluteUri.Host, StringComparison.OrdinalIgnoreCase);
        }
    }
}