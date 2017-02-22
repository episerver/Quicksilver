using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Web;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Controllers
{
    /// <summary>
    /// Base class for controllers related to ASP.NET Identity. This controller can be used both for
    /// pages and blocks.
    /// </summary>
    /// <typeparam name="T">The contextual IContent related to the current page or block.</typeparam>
    [AuthorizeContent]
    [VisitorGroupImpersonation]
    public abstract class IdentityControllerBase<T> : ActionControllerBase, IRenderTemplate<T> where T : IContentData
    {
        protected IdentityControllerBase(ApplicationSignInManager<SiteUser> applicationSignInManager, ApplicationUserManager<SiteUser> applicationUserManager, UserService userService)
        {
            SignInManager = applicationSignInManager;
            UserManager = applicationUserManager;
            UserService = userService;
        }

        public UserService UserService { get; private set; }

        public ApplicationSignInManager<SiteUser> SignInManager { get; private set; }

        public ApplicationUserManager<SiteUser> UserManager { get; private set; }

        /// <summary>
        /// Redirects the request to the original URL.
        /// </summary>
        /// <param name="returnUrl">The URL to be redirected to.</param>
        /// <returns>The ActionResult of the URL if it is within the current application, else it
        /// redirects to the web application start page.</returns>
        public ActionResult RedirectToLocal(string returnUrl)
        {
            if (returnUrl.IsLocalUrl(Request))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", new { node = ContentReference.StartPage });
        }

        [HttpGet]
        public ActionResult SignOut()
        {
            UserService.SignOut();
            return RedirectToAction("Index", new { node = ContentReference.StartPage });
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
            {
                return;
            }

            if (UserManager != null)
            {
                UserManager.Dispose();
            }

            if (SignInManager != null)
            {
                SignInManager.Dispose();    
            }

            if (UserService != null)
            {
                UserService.Dispose();
            }

            base.Dispose(disposing);

            _disposed = true;
        }
    }
}