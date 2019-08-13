using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters;
using EPiServer.SpecializedProperties;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class JsonSerializerExtensions
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter> {
                new XhtmlStringJsonConverter(),
                new ContentReferenceJsonConverter(),
                new LinkItemCollectionJsonConverter(),
                new ExcludeJsonConverter<PropertyContentReference>(),
                new ExcludeJsonConverter<PropertyDataCollection>()
            }
        };
        public static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(JsonSerializerSettings);
    }
}