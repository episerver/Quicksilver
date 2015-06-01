using System.Linq;
using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Navigation.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers
{
    public class HeaderController : Controller
    {
        private readonly IContentLoader _contentLoader;

        public HeaderController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        [ChildActionOnly]
        public ActionResult Index(IContent currentContent)
        {
            var model = new HeaderViewModel
            {
                CurrentContentLink = GetCategoryOrPageLink(currentContent),
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage)
            };
            return PartialView(model);
        }

        public ActionResult RightMenu(IContent currentContent)
        {
            var model = new HeaderViewModel
            {
                CurrentContentLink = currentContent != null ? currentContent.ContentLink : null,
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage)
            };
            return PartialView(model);
        }

        private ContentReference GetCategoryOrPageLink(IContent content)
        {
            CatalogContentBase catalogContent = content as CatalogContentBase;

            if (catalogContent != null)
            {
                return GetCategory(catalogContent);
            }

            if (content is PageData)
            { 
                return GetContentPage(content);
            }

            return null;
        }

        private ContentReference GetContentPage(IContent content)
        {
            return content.ContentLink;
        }

        private ContentReference GetCategory(CatalogContentBase content)
        {
            if (content is NodeContent)
            { 
                return content.ContentLink;
            }

            var category = _contentLoader.GetAncestors(content.ContentLink).Last(x => x is NodeContent);

            return category.ContentLink;
        }
    }
}