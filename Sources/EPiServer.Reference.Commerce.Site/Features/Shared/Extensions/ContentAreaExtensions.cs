using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class ContentAreaExtensions
    {
        public static bool IsNullOrEmpty(this ContentArea contentArea)
        {
            return contentArea == null || !contentArea.FilteredItems.Any();
        }

        public static List<BlockData> GetFilteredItems(this ContentArea contentArea)
        {
            var items = new List<BlockData>();

            if (contentArea.IsNullOrEmpty())
            {
                return items;
            }

            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

            foreach (var contentAreaItem in contentArea.FilteredItems)
            {
                IContentData item;
                if (!contentLoader.TryGet(contentAreaItem.ContentLink, out item) && !(item is BlockData))
                {
                    continue;
                }
                items.Add(item as BlockData);
            }

            return items;
        }
    }
}