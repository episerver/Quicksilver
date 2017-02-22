using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.ServiceLocation;
using Mediachase.BusinessFoundation;
using System.Web;

namespace EPiServer.Reference.Commerce.Manager
{
    public class SignInAsDifferentUserHandler : ICommand
    {
        public void Invoke(object sender, object element)
        {
            ServiceLocator.Current.GetInstance<ApplicationSignInManager<SiteUser>>().SignOut();
            HttpContext.Current.Response.Redirect("~/Apps/Shell/Pages/Login.aspx");
            HttpContext.Current.Response.End();
        }
    }
}