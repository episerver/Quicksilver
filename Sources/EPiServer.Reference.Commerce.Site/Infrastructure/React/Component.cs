﻿using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Newtonsoft.Json.Linq;
using System.Text;
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
                if(content is ContentData)
                {
                    jObject.AddFirst(new JProperty("$component", (content as ContentData).GetOriginalType().FullName));
                }
                else
                {
                    jObject.AddFirst(new JProperty("$component", content.GetType().FullName));
                }
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

        public static ActionResult RenderBlock<TContent>(TContent content) where TContent : class
        {
            return new JsonResult
            {
                Data = SerializeContent(content),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            };
        }
    }
}