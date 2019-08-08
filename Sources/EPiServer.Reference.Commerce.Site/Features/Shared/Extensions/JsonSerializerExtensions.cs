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
                new TempConverter<ContentReference>(),
                new TempConverter<PageReference>(),
                new TempConverter<LinkItemCollection>(),
                new TempConverter<BlockData>(),
                new TempConverter<XhtmlString>(),
                new TempConverter<ContentArea>(),
                new TempConverter<PropertyContentReference>(),
                new TempConverter<Url>(),
            }
        };

        public static string ToJson<T>(this T @object) where T : class
        => JsonConvert.SerializeObject(
            @object,
            @object.GetType(),
            Formatting.None,
            JsonSerializerSettings
        );
    }
}