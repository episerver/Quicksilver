using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Shared.Identity;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Mediachase.Data.Provider;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;

#if MIXED_MODE_AUTHENTICATION
[assembly: OwinStartup(typeof(EPiServer.Reference.Commerce.Site.Infrastructure.Owin.MixedModeAuthStartup))]
#endif
namespace EPiServer.Reference.Commerce.Site.Infrastructure.Owin
{
    public class MixedModeAuthStartup
    {
        // For more information on configuring mixed mode authentication with OpenID Connect and Membership provider,
        // please visit: https://world.episerver.com/documentation/developer-guides/commerce/security/support-for-openid-connect-in-episerver-commerce/
        // Note: The Katana team is working hard on updating performance and security however sometimes bugs are logged.
        // Please visit https://github.com/aspnet/AspNetKatana/issues to stay on top of issues that could affect your implementation. 

        private readonly IConnectionStringHandler _connectionStringHandler;

        public MixedModeAuthStartup() : this(ServiceLocator.Current.GetInstance<IConnectionStringHandler>())
        {
            // Parameterless constructor required by OWIN.
        }

        public MixedModeAuthStartup(IConnectionStringHandler connectionStringHandler)
        {
            _connectionStringHandler = connectionStringHandler;
        }

        public void Configuration(IAppBuilder app)
        {
            app.AddCmsAspNetIdentity<SiteUser>(new ApplicationOptions
            {
                ConnectionStringName = _connectionStringHandler.Commerce.Name
            });
                        
            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            app.SetDefaultSignInAsAuthenticationType(DefaultAuthenticationTypes.ExternalCookie);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AuthenticationType = DefaultAuthenticationTypes.ExternalCookie,
                CookieName = ".AspNet." + DefaultAuthenticationTypes.ExternalCookie,
                ExpireTimeSpan = TimeSpan.FromMinutes(5)
            });

            // <add key="ida:AADInstance" value="https://login.microsoftonline.com/{0}" />
            string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];

            // <add key="ida:ClientId" value="Client ID from Azure AD application" />
            string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

            // <add key="ida:Tenant" value="Azure Active Directory, Directory Eg. Contoso.onmicrosoft.com/" />
            string tenant = ConfigurationManager.AppSettings["ida:Tenant"];

            //<add key="ida:PostLogoutRedirectUri" value="https://the logout post uri/" />
            string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

            string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

            const string LogoutPath = "/Login/SignOut";

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = clientId,
                Authority = authority,
                RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"],
                PostLogoutRedirectUri = postLogoutRedirectUri,
                Scope = OpenIdConnectScopes.OpenIdProfile,
                ResponseType = OpenIdConnectResponseTypes.IdToken,
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    RoleClaimType = ClaimTypes.Role
                },
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = context =>
                    {
                        context.HandleResponse();
                        context.Response.Write(context.Exception.Message);
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = context =>
                    {
                        // Here you can change the return uri based on multisite
                        HandleMultiSiteReturnUrl(context);

                        // To avoid a redirect loop to the federation server send 403 
                        // when user is authenticated but does not have access
                        if (context.OwinContext.Response.StatusCode == 401 &&
                            context.OwinContext.Authentication.User.Identity.IsAuthenticated)
                        {
                            context.OwinContext.Response.StatusCode = 403;
                            context.HandleResponse();
                        }
                        return Task.FromResult(0);
                    },
                    SecurityTokenValidated = context =>
                    {
                        var redirectUri = new Uri(context.AuthenticationTicket.Properties.RedirectUri, UriKind.RelativeOrAbsolute);
                        if (redirectUri.IsAbsoluteUri)
                        {
                            context.AuthenticationTicket.Properties.RedirectUri = redirectUri.PathAndQuery;
                        }
                        //Sync user and the roles to EPiServer in the background
                        ServiceLocator.Current.GetInstance<ISynchronizingUserService>().
                        SynchronizeAsync(context.AuthenticationTicket.Identity);
                        return Task.FromResult(0);
                    }
                }
            });

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

            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"),
                Provider = new AuthorizationServerProvider(),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                AllowInsecureHttp = true,

            });

            // In case the site uses ServiceAPI, replace the below config with app.UseServiceApiIdentityTokenAuthorization<ApplicationUserManager<SiteUser>, SiteUser>(); 
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());


            app.UseStageMarker(PipelineStage.Authenticate);
            app.Map(LogoutPath, map =>
            {
                map.Run(ctx =>
                {
                    if (IsOpenIdConnectUser(ctx.Authentication.User))
                    {
                        ctx.Authentication.SignOut();
                    }
                    else
                    {
                        ctx.Get<ApplicationSignInManager<SiteUser>>().SignOut();
                    }
                    return Task.FromResult(0);
                });
            });

            // If the application throws an antiforgerytoken exception like “AntiForgeryToken: A Claim of Type NameIdentifier or IdentityProvider Was Not Present on Provided ClaimsIdentity”, 
            // set AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier.
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }

        private bool IsOpenIdConnectUser(ClaimsPrincipal user)
        {
            const string openIdConnectProviderClaimType = "http://schemas.microsoft.com/identity/claims/identityprovider";
            const string aspNetIdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider";
            return user.Claims.All(c => c.Type != aspNetIdentityProviderClaimType) && user.Claims.Any(c => c.Type == openIdConnectProviderClaimType);
        }

        private void HandleMultiSiteReturnUrl(
            RedirectToIdentityProviderNotification<OpenIdConnectMessage,
            OpenIdConnectAuthenticationOptions> context)
        {
            // here you change the context.ProtocolMessage.RedirectUri to corresponding siteurl
            // this is a sample of how to change redirecturi in the multi-tenant environment
            if (context.ProtocolMessage.RedirectUri == null)
            {
                var currentUrl = EPiServer.Web.SiteDefinition.Current.SiteUrl;
                context.ProtocolMessage.RedirectUri = new UriBuilder(
                   currentUrl.Scheme,
                   currentUrl.Host,
                   currentUrl.Port,
                   HttpContext.Current.Request.Url.AbsolutePath).ToString();
            }
        }
    }
}