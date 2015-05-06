using Microsoft.Owin.Security;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Login.Models
{
    /// <summary>
    /// Result class used for authenticating users against external login providers such as Facebook
    /// and Google+.
    /// </summary>
    internal class ChallengeResult : HttpUnauthorizedResult
    {
        private const string _xsrfKey = "XsrfId";

        /// <summary>
        /// Returns an instance of a new ChallengeResult.
        /// </summary>
        /// <param name="provider">The name of the external provider.</param>
        /// <param name="redirectUri">The Uri of the callback action method that should handle that response of the provider.</param>
        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        {
        }

        /// <summary>
        /// Returns an instance of a new ChallengeResult.
        /// </summary>
        /// <param name="provider">The name of the external provider.</param>
        /// <param name="redirectUri">The Uri of the callback action method that should handle that response of the provider.</param>
        /// <param name="userId">The id of the user.</param>
        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        /// <summary>
        /// Gets or sets the LoginProvider.
        /// </summary>
        public string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets the Uri of the callback action method that should handle that response of the provider.
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the UserId.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Calls the external provider and tries to authenticate the user.
        /// </summary>
        /// <param name="context">Encapsulated information about an HTTP request that matches 
        /// specified System.Web.Routing.RouteBase and System.Web.Mvc.ControllerBase instances.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
            {
                properties.Dictionary[_xsrfKey] = UserId;
            }
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }
}