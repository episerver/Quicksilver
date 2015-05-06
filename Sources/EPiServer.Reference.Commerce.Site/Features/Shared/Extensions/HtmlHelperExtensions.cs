using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static ProductSeparatorDiv BeginProductSeparator(this HtmlHelper htmlHelper, int totalProducts)
        {
            var separatorDiv = new ProductSeparatorDiv(htmlHelper.ViewContext, totalProducts);
            separatorDiv.WriteStart();
            return separatorDiv;
        }
    }
}