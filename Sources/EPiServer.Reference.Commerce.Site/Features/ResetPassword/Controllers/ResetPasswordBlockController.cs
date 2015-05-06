using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Blocks;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using EPiServer.Web.Routing;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordBlockController : BlockController<ResetPasswordBlock>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IMailSender _mailSender;
        private readonly IPasswordHashService _passwordHashService;

        public ResetPasswordBlockController(IContentLoader contentLoader, IMailSender mailSender, IPasswordHashService passwordHashService)
        {
            _contentLoader = contentLoader;
            _mailSender = mailSender;
            _passwordHashService = passwordHashService;
        }

        [HttpGet]
        public new ActionResult Index(ResetPasswordBlock currentBlock)
        {
            var pageRouteHelper = ServiceLocator.Current.GetInstance<PageRouteHelper>();
            var currentPageLink = pageRouteHelper.PageLink;
            var model = new ResetPasswordBlockViewModel
                {
                    CurrentBlock = currentBlock,
                    FormModel = new ResetPasswordBlockFormModel(),
                    CurrentPageLink = currentPageLink,
                    Success = false
                };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Index(ResetPasswordBlock currentBlock,
                                  ResetPasswordBlockFormModel formModel)
        {
            var model = new ResetPasswordBlockViewModel
                {
                    CurrentBlock = currentBlock,
                    FormModel = formModel
                };
            if (!ModelState.IsValid)
            {
                return PartialView(model);
            }

            var hash = _passwordHashService.CreateHash(formModel.Email);

            //Don't tell the user if the email exists.
            if (hash != null)
            {
                var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);

                _mailSender.Send(startPage.ResetPasswordMail, new NameValueCollection
                    {
                        {"hash", HttpServerUtility.UrlTokenEncode(hash)}
                    }, formModel.Email);
            }
            model.Success = true;
            return View(model);
        }
    }
}