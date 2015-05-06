using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Editor;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordMailController : PageController<ResetPasswordMailPage>
    {
        private readonly IContentLoader _contentLoader;

        public ResetPasswordMailController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public ActionResult Index(ResetPasswordMailPage currentPage, string hash)
        {
            if (string.IsNullOrEmpty(hash) && !PageEditing.PageIsInEditMode)
            {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(hash))
            {
                hash = "examplehash";
            }
            var model = new ResetPasswordMailModel
                {
                    CurrentPage = currentPage,
                    StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                    Hash = hash
                };
            return View(model);
        }
    }
}