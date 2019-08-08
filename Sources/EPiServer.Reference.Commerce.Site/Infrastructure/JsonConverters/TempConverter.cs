using Newtonsoft.Json;
using System;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters
{
    public class TempConverter<T> : JsonConverter<T> where T : class
    {
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return null;
        }

        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
        {
            writer.WriteValue("No Converter available This is in progress!");
        }
    }
}