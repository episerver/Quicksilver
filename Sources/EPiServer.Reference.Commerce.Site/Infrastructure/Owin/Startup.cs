using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Data.Provider;
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

[assembly: OwinStartup(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.Startup))]
namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
{
    public class Startup
    {
        // For more information on configuring authentication,
        // please visit http://world.episerver.com/documentation/Items/Developers-Guide/Episerver-CMS/9/Security/episerver-aspnetidentity/

        private readonly IConnectionStringHandler _connectionStringHandler;

        public Startup() : this(ServiceLocator.Current.GetInstance<IConnectionStringHandler>())
        {
            // Parameterless constructor required by OWIN.
        }

        public Startup(IConnectionStringHandler connectionStringHandler)
        {
            _connectionStringHandler = connectionStringHandler;
        }

        public void Configuration(IAppBuilder app)
        {
            app.AddCmsAspNetIdentity<SiteUser>(new ApplicationOptions
            {
                ConnectionStringName = _connectionStringHandler.Commerce.Name
            });

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
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager<SiteUser>, SiteUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => manager.GenerateUserIdentityAsync(user)),
                    OnApplyRedirect = (context => context.Response.Redirect(context.RedirectUri)),
                    OnResponseSignOut = (context => context.Response.Redirect(UrlResolver.Current.GetUrl(ContentReference.StartPage)))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // To enable using an external provider like Facebook or Google, uncomment the options you want to make available.
            // Also remember to apply the correct client id and secret code to each method that you call below.
            // Uncomment the external login providers you want to enable in your site. Don't forget to change their respective client id and secret.

            //EnableMicrosoftAccountLogin(app);
            //EnableTwitterAccountLogin(app);
            //EnableFacebookAccountLogin(app);
            //EnableGoogleAccountLogin(app);
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
                ClientId = "<ChangeThis>",
                ClientSecret = "<ChangeThis>",
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
                AppId = "<ChangeThis>",
                AppSecret = "<ChangeThis>"
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
                ConsumerKey = "<ChangeThis>",
                ConsumerSecret = "<ChangeThis>"
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
                ClientId = "<ChangeThis>",
                ClientSecret = "<ChangeThis>"
            };
            microsoftOptions.Scope.Add("email");
            app.UseMicrosoftAccountAuthentication(microsoftOptions);
        }
    }
}