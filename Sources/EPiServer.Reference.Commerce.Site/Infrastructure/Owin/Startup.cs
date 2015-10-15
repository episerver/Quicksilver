using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Models.Identity;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.Twitter;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web;

[assembly: OwinStartupAttribute(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.Startup))]
namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
{
    public class Startup
    {
        const string LogoutUrl = "/util/logout.aspx";

        public void Configuration(IAppBuilder app)
        {
            // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864

            // Configure the db context, user manager and signin manager to use a single instance per request.
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider.
            // Configure the sign in cookie.
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager)),
                    OnApplyRedirect = ApplyRedirect,
                    OnResponseSignedIn = context => ServiceLocator.Current.GetInstance<SynchronizingUserService>().SynchronizeAsync(context.Identity)
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            app.Map(LogoutUrl, map =>
            {
                map.Run(ctx =>
                {
                    ctx.Authentication.SignOut();
                    return Task.Run(() => ctx.Response.Redirect(UrlResolver.Current.GetUrl(ContentReference.StartPage)));
                });
            });

            // To enable using an external provider like Facebook or Google, uncomment the options you want to make available.
            // Also remember to apply the correct client id and secret code to each method that you call below.
            // Uncomment the external login providers you want to enable in your site. Don't forget to change their respective client id and secret.

            //EnableMicrosoftAccountLogin(app);
            //EnableTwitterAccountLogin(app);
            //EnableFacebookAccountLogin(app);
            //EnableGoogleAccountLogin(app);
        }

        /// <summary>
        /// Method for managing all the re-directs that occurs on the website.
        /// </summary>
        /// <param name="context"></param>
        private static void ApplyRedirect(CookieApplyRedirectContext context)
        {
            string backendPath = Paths.ProtectedRootPath.TrimEnd('/');

            // We use the method for transferring the user to the backend login pages if she tries to go
            // to the Edit views without being navigated.
            if (context.Request.Uri.AbsolutePath.StartsWith(backendPath) && !context.Request.User.Identity.IsAuthenticated)
            {
                context.RedirectUri = VirtualPathUtility.ToAbsolute("~/BackendLogin") +
                        new QueryString(
                            context.Options.ReturnUrlParameter,
                            context.Request.Uri.AbsoluteUri);
            }

            context.Response.Redirect(context.RedirectUri);
        }

        /// <summary>
        /// Enables authenticating users using their Google+ account.
        /// </summary>
        /// <remarks>
        /// To use this feature you need to have a developer account registered and a client project created at Google. You can do all
        /// this at https://console.developers.google.com.
        /// Once the client is created, copy the client id and client secret that is provided and put them as credentials in this
        /// method.
        /// Security Note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// </remarks>
        /// <param name="app">The application to associate with the login provider.</param>
        private static void EnableGoogleAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Google.
            GoogleOAuth2AuthenticationOptions googleOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "823672138190-qis91jbrccj3jat5rsmdmeb5k60n4rs9.apps.googleusercontent.com",
                ClientSecret = "usz9HoxNvedgSw1QZpwaGw1C",
            };
            googleOptions.Scope.Add("email");
            app.UseGoogleAuthentication(googleOptions);
        }

        /// <summary>
        /// Enables authenticating users using their Facebook account.
        /// </summary>
        /// <remarks>
        /// To use this feature you need to have a developer account registered and an app created at Facebook. You can do all
        /// this at https://developers.facebook.com/apps.
        /// Once the app is created, copy the client id and client secret that is provided and put them as credentials in this
        /// method.
        /// Security Note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// </remarks>
        /// <param name="app">The application to associate with the login provider.</param>
        private static void EnableFacebookAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Facebook.
            FacebookAuthenticationOptions facebookOptions = new FacebookAuthenticationOptions
            {
                AppId = "805286916226866",
                AppSecret = "34e50b02be4f1a7c62364c9fe89dcfd"
            };
            facebookOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookOptions);
        }

        /// <summary>
        /// Enables authenticating users using their Twitter account.
        /// </summary>
        /// <remarks>
        /// To use this feature you need to have a developer account registered and an app created at Twitter. You can do all
        /// this at https://dev.twitter.com/apps.
        /// Once the app is created, copy the client id and client secret that is provided and put them as credentials in this
        /// method.
        /// Security Note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// </remarks>
        /// <param name="app">The application to associate with the login provider.</param>
        private static void EnableTwitterAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Twitter.
            TwitterAuthenticationOptions twitterOptions = new TwitterAuthenticationOptions
            {
                ConsumerKey = "CqbBBscRO1jFbdr4CGRTiQFj",
                ConsumerSecret = "dh82r3SHGbL4ADgj6EJavdNGxFyx4YRX7QPyngBjKhV2bCdRrY"
            };
            app.UseTwitterAuthentication(twitterOptions);
        }

        /// <summary>
        /// Enables authenticating users using their Microsoft account.
        /// </summary>
        /// <remarks>
        /// To use this feature you need to have a developer account registered and an app created at Microsoft. You can do all
        /// this at https://account.live.com/developers/applications.
        /// Once the app is created, copy the client id and client secret that is provided and put them as credentials in this
        /// method.
        /// Security Note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// </remarks>
        /// <param name="app">The application to associate with the login provider.</param>
        private static void EnableMicrosoftAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Microsoft.
            MicrosoftAccountAuthenticationOptions microsoftOptions = new MicrosoftAccountAuthenticationOptions
            {
                ClientId = "0000000049673D31",
                ClientSecret = "hkQB8uwZ55HrkIcpeU9J6tXXebrMUpOS"
            };
            microsoftOptions.Scope.Add("email");
            app.UseMicrosoftAccountAuthentication(microsoftOptions);
        }
    }
}