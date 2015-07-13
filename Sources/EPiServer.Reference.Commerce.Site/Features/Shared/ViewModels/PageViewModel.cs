using EPiServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class PageViewModel<T> where T : PageData
    {
        public T CurrentPage { get; set; }
    }
}