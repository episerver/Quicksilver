using System.Collections.Generic;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Login;
using EPiServer.Reference.Commerce.Site.Features.Login.Models;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Web;
using EPiServer.Web.Mvc;

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
        /// <summary>
        /// Gets the user service used for working with user accounts.
        /// </summary>
        public UserService UserService
        {
            get { return _userService; }
        }

        /// <summary>
        /// The SignInManager sued when signing in users to their existing accounts.
        /// </summary>
        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager; }
        }

        /// <summary>
        /// The ApplicationUserManager used for creating new and retrieving existing users.
        /// </summary>
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

        /// <summary>
        /// Sign out the current user and redirects to the web site start page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SignOut()
        {
            UserService.SignOut();
            return RedirectToAction("Index", new { node = ContentReference.StartPage });
        }

        /// <summary>
        /// Adds any existing authentication errors to the ModelState.
        /// </summary>
        /// <param name="errors">The errors to be added.</param>
        public void AddErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
    }
}