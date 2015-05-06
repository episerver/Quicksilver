<%@ Application Language="C#" %>
<%@ Import Namespace="Mediachase.Commerce.Core.Dto" %>
<%@ Import Namespace="Mediachase.Commerce.Core" %>
<%@ Import Namespace="EPiServer.ServiceLocation" %>
<%@ Import Namespace="EPiServer.Framework.Initialization" %>
<%@ Import Namespace="EPiServer.Events.ChangeNotification" %>
<%@ Import Namespace="EPiServer.Logging" %>
<%@ Import Namespace="Mediachase.Commerce.Security" %>

<script RunAt="server">

    void Application_Start(object sender, EventArgs e)
    {
        // Code that runs on application startup
        Application["ComponentArtWebUI_AppKey"] = "This edition of ComponentArt Web.UI is licensed for EPiServer Framework only.";

        string[] resolvedPaths = new string[] {
			"~/Apps/MetaDataBase/Primitives/", 
			"~/Apps/MetaDataBase/MetaUI/Primitives/",
			"~/Apps/MetaUIEntity/Primitives/",
			"~/Apps/Customer/Primitives/"
		};

        Mediachase.Commerce.Manager.ControlPathResolver ctrlPathResolver = new Mediachase.Commerce.Manager.ControlPathResolver();

        ctrlPathResolver.Init(resolvedPaths);


        Mediachase.Commerce.Manager.ControlPathResolver.Current = ctrlPathResolver;

        Mediachase.Ibn.Web.UI.Layout.DynamicControlFactory.ControlsFolderPath = "~/Apps/";
        Mediachase.Ibn.Web.UI.Layout.WorkspaceTemplateFactory.ControlsFolderPath = "~/Apps/";
    }

    void Application_End(object sender, EventArgs e)
    {
    }

    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError().GetBaseException();

        if (ex != null)
        {
            if (typeof(AccessDeniedException) == ex.GetType())
            {
                Response.Redirect(String.Format("~/Apps/Shell/Pages/Unauthorized.html"));
            }
            else if (typeof(HttpException) == ex.GetType())
            {
                int errorCode = ((HttpException)ex).ErrorCode;
                if (errorCode == 500) // consider 500 a fatal exception
                {
                    // Log the exception
                    LogManager.GetLogger(GetType()).Critical("Backend encountered unhandled error.", ex);
                    return;
                }
            }
        }

        // Code that runs when an unhandled error occurs
        // Log the exception
        LogManager.GetLogger(GetType()).Error("Backend encountered unhandled error.", ex);

    }

    void Session_Start(object sender, EventArgs e)
    {
        // Code that runs when a new session is started
    }

    void Session_End(object sender, EventArgs e)
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

        //Unlock all user locked objects
        Mediachase.Commerce.Orders.Managers.OrderGroupLockManager.UnlockAllUserLocks(EPiServer.Security.PrincipalInfo.CurrentPrincipal.GetContactId());

    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        log4net.ThreadContext.Properties["Hostname"] = HttpContext.Current.Request.UserHostAddress;
        // Bug fix for MS SSRS Blank.gif 500 server error missing parameter IterationId
        if (HttpContext.Current.Request.Url.PathAndQuery.StartsWith("/Reserved.ReportViewerWebControl.axd") &&
         !String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["ResourceStreamID"]) &&
            HttpContext.Current.Request.QueryString["ResourceStreamID"].ToLower().Equals("blank.gif"))
        {
            Context.RewritePath(String.Concat(HttpContext.Current.Request.Url.PathAndQuery, "&IterationId=0"));
        }
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {

    }

    protected void Application_AuthorizeRequest(object sender, EventArgs e)
    {
        HttpApplication httpApplication = (HttpApplication)sender;

        if (this.Request.IsAuthenticated)
        {
            // Check current 
            string fullName = User.Identity.Name;
            string appName = String.Empty;

            // If user authenticated, recreate the authentication cookie with a new value
            HttpCookie cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                appName = ticket.UserData;
            }

            if (appName.Length == 0)
            {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
                httpApplication.CompleteRequest();
            }

            AppDto dto = Mediachase.Commerce.Core.AppContext.Current.GetApplicationDto(appName);

            // If application does not exists or is not active, prevent login
            if (dto == null || dto.Application.Count == 0 || !dto.Application[0].IsActive)
            {
                FormsAuthentication.SignOut();
                FormsAuthentication.RedirectToLoginPage();
                httpApplication.CompleteRequest();
            }
            else
            {
                Membership.Provider.ApplicationName = appName;
                Roles.Provider.ApplicationName = appName;
                ProfileManager.ApplicationName = appName;
                Mediachase.Commerce.Core.AppContext.Current.ApplicationId = dto.Application[0].ApplicationId;
                Mediachase.Commerce.Core.AppContext.Current.ApplicationName = dto.Application[0].Name;
                log4net.ThreadContext.Properties["ApplicationId"] = Mediachase.Commerce.Core.AppContext.Current.ApplicationId;
                log4net.ThreadContext.Properties["Username"] = Mediachase.Commerce.Security.SecurityContext.Current.CurrentUserName;
            }

            // Check permissions
            // Check permissions
            if (Mediachase.Commerce.Security.SecurityContext.Current.IsPermissionCheckEnable)
            {
                if (!Mediachase.Commerce.Security.SecurityContext.Current.CheckPermissionForCurrentUser("core:mng:login"))
                {
                    FormsAuthentication.SignOut();
                    this.Response.Redirect("~/Apps/Shell/Pages/Unauthorized.html");
                    httpApplication.CompleteRequest();
                }

                Mediachase.Commerce.Security.SecurityContext context = Mediachase.Commerce.Security.SecurityContext.Current;

                try
                {
                    if (context != null)
                    {
                        Mediachase.Commerce.Customers.Profile.CustomerProfileWrapper profile = context.CurrentUserProfile as Mediachase.Commerce.Customers.Profile.CustomerProfileWrapper;

                        if (profile != null && profile.State != 2)
                        {
                            FormsAuthentication.SignOut();
                            this.Response.Redirect("~/Apps/Shell/Pages/Unauthorized.html");
                            httpApplication.CompleteRequest();
                        }
                    }
                }
                catch (System.Data.SqlClient.SqlException)
                {
                    FormsAuthentication.SignOut();
                    FormsAuthentication.RedirectToLoginPage();
                    httpApplication.CompleteRequest();
                }
            }
            else if (!Mediachase.Commerce.Security.SecurityContext.Current.CheckCurrentUserInAnyGlobalRoles(
                new string[] { Mediachase.Commerce.Core.AppRoles.AdminRole, Mediachase.Commerce.Core.AppRoles.ManagerUserRole }))
            {
                FormsAuthentication.SignOut();
                this.Response.Redirect("~/Apps/Shell/Pages/Unauthorized.html");
                httpApplication.CompleteRequest();
            }
        }
    }

    protected void Application_PostAcquireRequestState(object sender, EventArgs e)
    {
        try
        {
            SetCulture(Mediachase.Web.Console.ManagementContext.Current.ConsoleUICulture);
        }
        catch (Exception)
        {
        }
    }

    public static void SetCulture(System.Globalization.CultureInfo culture)
    {
        // Set the CurrentCulture property to the requested culture.
        System.Threading.Thread.CurrentThread.CurrentCulture = culture;

        // Initialize the CurrentUICulture property
        // with the CurrentCulture property.
        System.Threading.Thread.CurrentThread.CurrentUICulture = System.Threading.Thread.CurrentThread.CurrentCulture;
    }
</script>
