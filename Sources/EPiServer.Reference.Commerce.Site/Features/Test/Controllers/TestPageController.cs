using EPiServer.Reference.Commerce.Site.Infrastructure.React;
using EPiServer.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Test.Pages;
using EPiServer.Reference.Commerce.Site.Features.Test.ViewModels;

namespace EPiServer.Reference.Commerce.Site.Features.Test.Controllers
{
    public class TestPageController : PageController<TestPage>
    {
        // GET: TesPage
        public ActionResult Index(TestPage currentPage)
        {
            var model = new TestPageViewModel
            {
                CurrentPage = currentPage,
                Title = "TEST TITLE",
            };
            return Component.RenderPage(model);
        }
    }
}