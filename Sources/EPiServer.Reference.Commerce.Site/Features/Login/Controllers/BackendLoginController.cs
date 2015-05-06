using EPiServer.Commerce.Internal.Migration;
using EPiServer.Data.Dynamic;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Login.Models;
using EPiServer.Reference.Commerce.Site.Features.Login.ViewModels;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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
        /// <summary>
        /// The default action method when signing in to the web site EDIT mode.
        /// </summary>
        /// <returns>The login view prompting the end-user for username and password.</returns>
        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            BackendLoginViewModel viewModel = new BackendLoginViewModel() { ReturnUrl = returnUrl };
            viewModel.Heading = LocalizationService.Current.GetString("/Login/BackendLogin/Heading");
            viewModel.LoginMessage = LocalizationService.Current.GetString("/Login/BackendLogin/LoginMessage");

            return View(viewModel);
        }

        /// <summary>
        /// Authenticates a user's password and username.
        /// </summary>
        /// <param name="viewModel">View model holding the user's credentials.</param>
        /// <returns>If the login is successfull </returns>
        [HttpPost]
        public async Task<ActionResult> Index(BackendLoginViewModel viewModel)
        {
            ApplicationSignInManager signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            string returnUrl = !string.IsNullOrEmpty(viewModel.ReturnUrl) ? viewModel.ReturnUrl : "/";

            if (!ModelState.IsValid)
            {
                // re-apply the messages for the login view.
                viewModel.Heading = LocalizationService.Current.GetString("/Login/BackendLogin/Heading");
                viewModel.LoginMessage = LocalizationService.Current.GetString("/Login/BackendLogin/LoginMessage");

                return PartialView("Index", viewModel);
            }

            var result = await signInManager.PasswordSignInAsync(viewModel.Username, viewModel.Password, viewModel.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    break;

                case SignInStatus.LockedOut:
                case SignInStatus.RequiresVerification:
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("Password", LocalizationService.Current.GetString("/Login/Form/Error/WrongPasswordOrEmail"));
                    return PartialView("Index", viewModel);
            }

            // As a security concern in order to prevent open re-direct attacks we
            // check the return URL to make sure it is within the own site. The method
            // Url.IsLocalUrl does not recoqnize localhost as true, so to make this work while
            // debugging we should also allow calls coming from within the same server. 
            // We can do this by first checking with Request.IsLocal.
            if (Request.IsLocal || Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // If the return URL was set to an external address then make sure the call goes to the
            // start page of the site instead.
            return RedirectToAction("Index", new { node = EPiServer.Core.ContentReference.StartPage });
        }
    }
}