using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Newtonsoft.Json;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters
{
    public class ContentReferenceJsonConverter : JsonConverter<ContentReference>
    {
        public override ContentReference ReadJson(JsonReader reader, Type objectType, ContentReference existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, ContentReference value, JsonSerializer serializer)
        {
            if (ContentReference.IsNullOrEmpty(value))
            {
                writer.WriteValue(string.Empty);
                return;
            }

            var urlResolver = ServiceLocator.Current.GetInstance<UrlResolver>();

            writer.WriteValue(urlResolver.GetUrl(value));
        }
    }
}