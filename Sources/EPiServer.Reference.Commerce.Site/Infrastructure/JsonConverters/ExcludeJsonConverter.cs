using Newtonsoft.Json;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters
{
    public class ExcludeJsonConverter<T> : JsonConverter<T> where T : class
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            // No json value 
            writer.WriteNull();
        }
    }
}