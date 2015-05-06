using EPiServer.Core;
using EPiServer.SpecializedProperties;
using EPiServer.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Extensions
{
    public static class LinkItemExtensions
    {
        public static ContentReference GetContentReference(this LinkItem linkItem)
        {
            string extension;
            var guid = PermanentLinkUtility.GetGuid(new UrlBuilder(linkItem.GetMappedHref()), out extension);
            return PermanentLinkUtility.FindContentReference(guid);
        }
    }
}