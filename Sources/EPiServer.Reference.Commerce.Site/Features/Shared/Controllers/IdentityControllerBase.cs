using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
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
        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;
        private readonly UserService _userService;

        protected IdentityControllerBase(ApplicationSignInManager applicationSignInManager, ApplicationUserManager applicationUserManager, UserService userService)
        {
            _signInManager = applicationSignInManager;
            _userManager = applicationUserManager;
            _userService = userService;
        }

        public UserService UserService
        {
            get { return _userService; }
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager; }
        }

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

            if (_userManager != null)
            {
                _userManager.Dispose();
            }

            if (_signInManager != null)
            {
                _signInManager.Dispose();    
            }

            if (_userService != null)
            {
                _userService.Dispose();
            }

            base.Dispose(disposing);

            _disposed = true;
        }
    }
}