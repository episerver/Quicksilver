using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class HashTableJsonConverter : JsonConverter
    {
        const string TypeFieldName = "$type";
        const string ValueFieldName = "$value";

        public override bool CanConvert(Type objectType)
        {
            return typeof(Hashtable).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            Hashtable ht;

            if (!(existingValue is Hashtable))
            {
                ht = (objectType == typeof(Hashtable))
                    ? new Hashtable()
                    : (Hashtable)Activator.CreateInstance(objectType);
            }
            else
            {
                ht = existingValue as Hashtable;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException($"Unexpected JSON token when reading HashTable. Expected StartObject, got {reader.TokenType}, path: {reader.Path}.");
            }

            reader.Read();

            while (reader.TokenType != JsonToken.EndObject)
            {
                ReadHashtableItem(reader, ht, serializer);
                reader.Read();
            }

            return ht;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var properties = (Hashtable)value;

            writer.WriteStartObject();

            foreach (var key in properties.Keys)
            {
                writer.WritePropertyName(key.ToString());

                var property = properties[key];

                if (property == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName(TypeFieldName);
                    writer.WriteValue(property.GetType().FullName);

                    writer.WritePropertyName(ValueFieldName);
                    serializer.Serialize(writer, property);

                    writer.WriteEndObject();
                }
            }

            writer.WriteEndObject();
        }

        private static void ReadHashtableItem(JsonReader reader, Hashtable hashTable, JsonSerializer serializer)
        {
            // start reading next hash-table item
            if (reader.TokenType != JsonToken.PropertyName)
            {
                throw new JsonSerializationException($"Unexpected JSON token when reading HashTable. Expected PropertyName, got {reader.TokenType}, path: {reader.Path}.");
            }

            var propertyName = reader.Value;

            reader.Read();

            if (reader.TokenType != JsonToken.StartObject)
            {
                // for hashtable that serialized using default converter instead of this custom one
                // serialized string doesn't contain specific type name, so return value as default.
                hashTable.Add(propertyName, serializer.Deserialize(reader));
                return;
            }

            var dataTypeValue = string.Empty;

            reader.Read();

            var jsonObject = new JObject();

            while (reader.TokenType != JsonToken.EndObject)
            {
                var elementName = reader.Value.ToString();
                reader.Read();

                if (elementName == TypeFieldName)
                {
                    dataTypeValue = reader.Value.ToString();
                }
                else if (elementName == ValueFieldName)
                {
                    // make sure that we have read type field before value field
                    // then the value can be converted to correct type.

                    if (!string.IsNullOrEmpty(dataTypeValue))
                    {
                        Type dataType = Type.GetType(dataTypeValue);

                        var dataValueDeserialized = (dataType == null) ?
                            serializer.Deserialize(reader) :
                            serializer.Deserialize(reader, dataType);

                        hashTable.Add(propertyName, dataValueDeserialized);
                    }
                }

                jsonObject.Add(elementName, reader.Value != null ? JToken.FromObject(serializer.Deserialize(reader)) : null);

                reader.Read();
            }

            if (string.IsNullOrEmpty(dataTypeValue))
            {
                // in case of legacy object, no $type or $value is found. Return jsonObject value in hashtable.

                hashTable.Add(propertyName, jsonObject);
            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonSerializationException($"Unexpected JSON token when reading HashTable. Expected EndObject, got {reader.TokenType}, path: {reader.Path}.");
            }
        }
    }
}
