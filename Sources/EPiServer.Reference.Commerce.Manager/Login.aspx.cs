using EPiServer.Reference.Commerce.Shared.Models.Identity;
using Mediachase.Commerce.Core;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPiServer.Reference.Commerce.Manager
{
    public partial class Login : Page
    {
        private const string UserLoginFailureMessage = "Login failed. Please try again.";

        private ApplicationSignInManager _signInManager;

        protected void Page_Load(object sender, EventArgs e)
        {
            _signInManager = Request.GetOwinContext().Get<ApplicationSignInManager>();
            LoginCtrl.Authenticate += LoginCtrl_Authenticate;

            if (IsPostBack) 
            {
                return;
            }
            LoginCtrl.FindControl("ApplicationRow").Visible = AppContext.Current.GetApplicationDto().Application.Count != 1;
            LoginCtrl.Focus();
        }

        protected void LoginCtrl_Authenticate(object sender, AuthenticateEventArgs e)
        {
            var userName = ((TextBox)LoginCtrl.FindControl("UserName")).Text;
            var password = ((TextBox)LoginCtrl.FindControl("Password")).Text;
            var remember = ((CheckBox)LoginCtrl.FindControl("RememberMe")).Checked;

            var validated = _signInManager.PasswordSignInAsync(userName, password, remember, false).Result == SignInStatus.Success;
            if (validated) 
            {
                HandleLoginSuccess(userName, remember);
            }
            else
            {
                HandleLoginFailure(UserLoginFailureMessage);
            }
        }

        private void HandleLoginSuccess(string userName, bool remember)
        {
            string url = FormsAuthentication.GetRedirectUrl(userName, remember);
            if (url.Equals(FormsAuthentication.DefaultUrl, StringComparison.OrdinalIgnoreCase) ||
                url.Contains(".axd") ||
                url.Contains("/Apps/Core/Controls/Uploader/")) 
            {
                url = "~/Apps/Shell/Pages/default.aspx";
            }

            Response.Redirect(url);
        }

        private void HandleLoginFailure(string pageMessage)
        {
            LoginCtrl.FailureText = pageMessage;
        }
    }
}