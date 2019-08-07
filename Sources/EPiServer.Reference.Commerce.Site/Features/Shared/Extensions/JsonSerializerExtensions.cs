using System;
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
        };

        public static string ToJson(this object @object)
        => JsonConvert.SerializeObject(
            @object,
            @object.GetType(),
            Formatting.None,
            JsonSerializerSettings
        );
    }
}