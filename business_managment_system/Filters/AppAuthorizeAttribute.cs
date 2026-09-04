using System.Web.Mvc;
using System.Web.Routing;

namespace business_managment_system.Filters
{
    public class AppAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Controller.TempData["Error"] = "You do not have access to that page.";
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
