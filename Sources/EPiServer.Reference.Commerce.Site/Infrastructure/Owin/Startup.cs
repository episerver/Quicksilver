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

#if !(MIXED_MODE_AUTHENTICATION)
[assembly: OwinStartup(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.Startup))]
#endif
namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
{
    public class Startup
    {
        // For more information on configuring authentication,
        // please visit http://world.episerver.com/documentation/Items/Developers-Guide/Episerver-CMS/9/Security/episerver-aspnetidentity/
        // For more information on configuring OpenID Connect,
        // please visit https://world.episerver.com/documentation/developer-guides/commerce/security/support-for-openid-connect-in-episerver-commerce/
        // Note: The Katana team is working hard on updating performance and security however sometimes bugs are logged.
        // Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation. 

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
                    OnApplyRedirect = context => context.Response.Redirect(context.RedirectUri),
                    OnResponseSignOut = context => context.Response.Redirect(UrlResolver.Current.GetUrl(ContentReference.StartPage))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

#if GOOGLE_ACCOUNT_LOGIN_FEATURE
            EnableGoogleAccountLogin(app);
#endif

#if FACEBOOK_ACCOUNT_LOGIN_FEATURE
            EnableFacebookAccountLogin(app);
#endif

#if TWITTER_ACCOUNT_LOGIN_FEATURE
            EnableTwitterAccountLogin(app);
#endif

#if MICROSOFT_ACCOUNT_LOGIN_FEATURE
            EnableMicrosoftAccountLogin(app);
#endif
        }

        /// <summary>
        /// Enables authenticating users using their Google+ account.
        /// </summary>
        /// <param name="app">The application to associate with the login provider.</param>
        /// <remarks>
        /// To use this feature, define GOOGLE_ACCOUNT_LOGIN_FEATURE symbol.
        /// A Google+ developer account and an app needs to be created at https://console.developers.google.com.
        /// Update ClientId and ClientSecret values with provided client id and client secret.
        /// Security note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// Note: Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation.
        /// </remarks>
        private static void EnableGoogleAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Google.
            var googleOptions = new GoogleOAuth2AuthenticationOptions()
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
        /// <param name="app">The application to associate with the login provider.</param>
        /// <remarks>
        /// To use this feature, define FACEBOOK_ACCOUNT_LOGIN_FEATURE symbol.
        /// A Facebook developer account and an app needs to be created at https://developers.facebook.com/apps.
        /// Update AppId and AppSecret values with provided client id and client secret.
        /// Security note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// Note: Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation.
        /// </remarks>
        private static void EnableFacebookAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Facebook.
            var facebookOptions = new FacebookAuthenticationOptions
            {
                AppId = "<ChangeThis>",
                AppSecret = "<ChangeThis>"
            };
            facebookOptions.Scope.Add("public_profile");
            facebookOptions.Scope.Add("email");
            facebookOptions.Fields.Add("email");
            facebookOptions.Fields.Add("name");
            app.UseFacebookAuthentication(facebookOptions);
        }

        /// <summary>
        /// Enables authenticating users using their Twitter account.
        /// </summary>
        /// <param name="app">The application to associate with the login provider.</param>
        /// <remarks>
        /// To use this feature, define TWITTER_ACCOUNT_LOGIN_FEATURE symbol.
        /// A Twitter developer account and an app needs to be created at https://dev.twitter.com/apps.
        /// Update ConsumerKey and ConsumerSecret values with provided client id and client secret.
        /// Security note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// Note: Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation.
        /// </remarks>
        private static void EnableTwitterAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Twitter.
            var twitterOptions = new TwitterAuthenticationOptions
            {
                ConsumerKey = "<ChangeThis>",
                ConsumerSecret = "<ChangeThis>"
            };
            app.UseTwitterAuthentication(twitterOptions);
        }

        /// <summary>
        /// Enables authenticating users using their Microsoft account.
        /// </summary>
        /// <param name="app">The application to associate with the login provider.</param>
        /// <remarks>
        /// To use this feature, define MICROSOFT_ACCOUNT_LOGIN_FEATURE symbol.
        /// A Microsoft developer account and an app needs to be created at https://account.live.com/developers/applications.
        /// Update ClientId and ClientSecret values with provided client id and client secret.
        /// Security note: Never store sensitive data in your source code. The account and credentials are added to the code above to keep the sample simple.
        /// Note: Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation.
        /// </remarks>
        private static void EnableMicrosoftAccountLogin(IAppBuilder app)
        {
            // Note that the id and secret code below are fictitious and will not work when calling Microsoft.
            var microsoftOptions = new MicrosoftAccountAuthenticationOptions
            {
                ClientId = "<ChangeThis>",
                ClientSecret = "<ChangeThis>"
            };
            microsoftOptions.Scope.Add("email");
            app.UseMicrosoftAccountAuthentication(microsoftOptions);
        }
    }
}