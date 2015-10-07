using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Controllers
{
    /// <summary>
    /// Controller used for back-end user authentication.
    /// </summary>
    /// <remarks>
    /// The reason for this controller is because the default login is dependent on existing content data
    /// and during the execution of the initial migration steps the standard LoginController can't be
    /// created. That is why this has been implemented as an ordinary MVC controller instead and used
    /// whenever a user is trying to login against the backend Edit views.
    /// </remarks>
    public class BackendLoginController : Controller
    {
        private readonly LocalizationService _localizationService;
        private readonly ApplicationSignInManager _signInManager;
        private readonly UrlAuthorizationFacade _urlAuthorization;

        public BackendLoginController(LocalizationService localizationService, 
            ApplicationSignInManager signInManager,
            UrlAuthorizationFacade urlAuthorization)
        {
            _localizationService = localizationService;
            _signInManager = signInManager;
            _urlAuthorization = urlAuthorization;
        }

        /// <summary>
        /// The default action method when signing in to the web site EDIT mode.
        /// </summary>
        /// <returns>The login view prompting the end-user for username and password.</returns>
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            var viewModel = new BackendLoginViewModel
            {
                ReturnUrl = returnUrl,
                Heading = _localizationService.GetString("/Login/BackendLogin/Heading"),
                LoginMessage = _localizationService.GetString("/Login/BackendLogin/LoginMessage")
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Index(BackendLoginViewModel viewModel)
        {
            var returnUrl = !string.IsNullOrEmpty(viewModel.ReturnUrl) ? viewModel.ReturnUrl : UrlHelper.GenerateContentUrl("~/", ControllerContext.HttpContext);

            if (!ModelState.IsValid)
            {
                // re-apply the messages for the login view.
                viewModel.Heading = _localizationService.GetString("/Login/BackendLogin/Heading");
                viewModel.LoginMessage = _localizationService.GetString("/Login/BackendLogin/LoginMessage");

                return PartialView("Index", viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, viewModel.RememberMe, false);

            switch (result)
            {
                case SignInStatus.Success:
                    break;
                default:
                    ModelState.AddModelError("Password", _localizationService.GetString("/Login/Form/Error/WrongPasswordOrEmail"));
                    return PartialView("Index", viewModel);
            }
            
            // As a security concern in order to prevent open re-direct attacks we
            // check the return URL to make sure it is within the own site. The method
            // Url.IsLocalUrl does not recognize localhost as true, so to make this work while
            // debugging we should also allow calls coming from within the same server. 
            // We can do this by first checking with Request.IsLocal.
            if (Request.IsLocal || returnUrl.IsLocalUrl(Request))
            {
                return Redirect(returnUrl);
            }

            // If the return URL was set to an external address then make sure the call goes to the
            // start page of the site instead.
            return RedirectToAction("Index", new { node = Core.ContentReference.StartPage });
        }
    }
}