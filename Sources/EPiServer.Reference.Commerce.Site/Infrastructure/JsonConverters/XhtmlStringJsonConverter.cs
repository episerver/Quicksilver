using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.JsonConverters
{
    //Todo: Enhance the fetching of blocks
    public class XhtmlStringJsonConverter : JsonConverter<XhtmlString>
    {
        public static List<Type> Controllers = Assembly.GetExecutingAssembly().GetTypes().Where(w => w.BaseType != null && w.BaseType.Name.StartsWith("BlockController")).ToList();
        public override XhtmlString ReadJson(JsonReader reader, Type objectType, XhtmlString existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, XhtmlString value, JsonSerializer serializer)
        {
            var result = new JObject();

            if (!(value is ContentArea))
            {
                result.Add(new JProperty("$propertyType", "XhtmlString"));
                result.Add(new JProperty("value", value.ToString()));
                result.WriteTo(writer);
                return;
            }

            var contentArea = value as ContentArea;
            result.Add(new JProperty("$propertyType", "ContentArea"));

            if (contentArea.IsNullOrEmpty())
            {
                result.Add(new JProperty("items", Enumerable.Empty<string>()));
                result.WriteTo(writer);
                return;
            }
            var items = new List<object>();
            var blocks = contentArea.GetFilteredItems();

            foreach (var block in blocks)
            {
                var currentType = block.GetOriginalType();
                var typeOfController = Controllers.SingleOrDefault(w => w.Name.Equals($"{currentType.Name}Controller"));

                if (typeOfController == null)
                {
                    items.Add(JObject.FromObject(block, serializer));
                    continue;
                }

                var controller = ServiceLocator.Current.GetInstance(typeOfController) as ActionControllerBase;
                var indexMethod = typeOfController.GetMethod("Index");
                var indexResult = indexMethod.Invoke(controller, new[] { block }) as JsonResult;

                items.Add(indexResult.Data);
            }

            result.Add(new JProperty("items", JArray.FromObject(items, serializer)));
            result.WriteTo(writer);
        }
    }
}