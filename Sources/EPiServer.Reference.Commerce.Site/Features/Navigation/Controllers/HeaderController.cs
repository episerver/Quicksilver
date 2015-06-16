using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Navigation.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Mediachase.Commerce.Customers;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Navigation.Controllers
{
    public class HeaderController : Controller
    {
        private readonly IContentLoader _contentLoader;
        private readonly CurrentContactFacade _currentContactFacade;

        public HeaderController(CurrentContactFacade currentContactFacade, IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
            _currentContactFacade = currentContactFacade;
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
            string userDisplayName = null;
            CustomerContact customer = _currentContactFacade.CurrentContact;

            if (customer != null)
            {
                userDisplayName = customer.FirstName + " " + customer.LastName;
            }

            var model = new HeaderViewModel
            {
                CurrentContentLink = currentContent != null ? currentContent.ContentLink : null,
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                UserDisplayName = userDisplayName
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
                return content.ContentLink;
            }

            return null;
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