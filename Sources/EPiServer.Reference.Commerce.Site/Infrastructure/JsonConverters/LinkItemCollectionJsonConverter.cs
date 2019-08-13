using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters
{
    public class LinkItemCollectionJsonConverter : JsonConverter<LinkItemCollection>
    {
        public override LinkItemCollection ReadJson(JsonReader reader, Type objectType, LinkItemCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, LinkItemCollection value, JsonSerializer serializer)
        {
            var result = new JObject();
            result.Add(new JProperty("$propertyType", "LinkItemCollection"));
            var items = new List<LinkItemViewModel>();
            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();
            foreach (var item in value)
            {
                var linkModel = new LinkItemViewModel
                {
                    Text = item.Text,
                    Target = item.Target,
                    Title = item.Title,
                    Href = item.Href,
                };
                var contentLink = urlResolver.Route(new UrlBuilder(item.Href))?.ContentLink;
                if (!ContentReference.IsNullOrEmpty(contentLink))
                {
                    linkModel.Href = urlResolver.GetUrl(contentLink);
                }
                items.Add(linkModel);
            }
            result.Add(new JProperty("items", JArray.FromObject(items, serializer)));
            result.WriteTo(writer);
        }
    }
}