using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;

namespace EPiServer.Reference.Commerce.Site.Infrastructure.React
{
    public static class Component
    {
        public static JObject SerializeContent<TContent>(TContent content) where TContent : class
        {
            var jObject = JObject.FromObject(content, JsonSerializerExtensions.JsonSerializer);
            if (!jObject.ContainsKey("$component"))
            {
                var fullname = string.Empty;
                if (content is ContentData)
                {
                    fullname = (content as ContentData).GetOriginalType().FullName;
                }
                else
                {
                    fullname = content.GetType().FullName;
                }
                if (fullname.Contains("`"))
                {
                    fullname = fullname.Substring(0, fullname.IndexOf('`'));
                }
                jObject.AddFirst(new JProperty("$component", fullname));
            }
            return jObject;
        }

        public static ActionResult RenderPage<TContent>(TContent content) where TContent : class
        {

            if (!HttpContext.Current.Request.QueryString.AllKeys.Contains("$json"))
            {
                var result = new ViewResult
                {
                    ViewName = "_Empty",
                    ViewData = new ViewDataDictionary(content),
                };
                result.ViewData.Add("CURRENT_PAGE", SerializeContent(content));

                return result;
            }

            return new JsonResult
            {
                Data = SerializeContent(content),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }

        public static ActionResult RenderJson<TContent>(TContent content) where TContent : class
        {
            return new ContentResult
            {
                Content = SerializeContent(content).ToString(),
                ContentType = "application/json",
            };
        }
    }
}