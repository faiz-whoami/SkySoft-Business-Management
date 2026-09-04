using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace business_managment_system
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
            {
                return;
            }

            try
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null || ticket.Expired)
                {
                    return;
                }

                var identity = new FormsIdentity(ticket);
                var roles = string.IsNullOrWhiteSpace(ticket.UserData)
                    ? new string[0]
                    : new[] { ticket.UserData };
                Context.User = new GenericPrincipal(identity, roles);
            }
            catch (Exception)
            {
                // Ignore a bad cookie and treat the request as anonymous.
            }
        }
    }
}
