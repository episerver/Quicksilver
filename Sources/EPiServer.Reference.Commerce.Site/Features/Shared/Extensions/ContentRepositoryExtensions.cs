using System.Linq;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class ContentRepositoryExtensions
    {
        public static T GetFirstChild<T>(this IContentRepository contentRepository, ContentReference contentReference) where T : IContentData
        {
            return contentRepository.GetChildren<T>(contentReference).FirstOrDefault();
        }
    }
}