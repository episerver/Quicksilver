using EPiServer.Reference.Commerce.Site.Features.Search.Controllers;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Extensions
{
    public class ProductSeparatorDiv : IDisposable
    {
        private readonly ViewContext _viewContext;
        private bool _disposed;
        private readonly TagBuilder _div;
        private readonly Dictionary<int, string> _totalToCssClassMap = new Dictionary<int, string>{
            // two products, each should have half width
            {WideWidthCategoryPartialController.NumberOfProducts, Constants.ContentAreaTags.HalfWidth}, 
            // three products, each should have one third width
            {FullWidthCategoryPartialController.NumberOfProducts, Constants.ContentAreaTags.OneThirdWidth} 
        };

        public ProductSeparatorDiv(ViewContext viewContext, int totalProducts)
        {
            if (viewContext == null)
            {
                throw new ArgumentNullException("viewContext");
            }
            _viewContext = viewContext;
            _div = new TagBuilder("div");
            string cssClass = string.Empty;
            if (_totalToCssClassMap.TryGetValue(totalProducts, out cssClass))
            {
                _div.AddCssClass(cssClass);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize((object) this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            WriteEnd();
        }

        public void WriteStart()
        {
            _viewContext.Writer.Write(_div.ToString(TagRenderMode.StartTag));
        }

        public void WriteEnd()
        {
            _viewContext.Writer.Write(_div.ToString(TagRenderMode.EndTag));
        }
    }
}