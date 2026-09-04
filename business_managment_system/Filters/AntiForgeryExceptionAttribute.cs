using System.Web.Mvc;
using System.Web.Routing;
using business_managment_system.Helpers;

namespace business_managment_system.Filters
{
    public class AntiForgeryExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null || filterContext.ExceptionHandled)
            {
                return;
            }

            if (!(filterContext.Exception is HttpAntiForgeryException))
            {
                return;
            }

            AntiForgeryCookie.Expire(filterContext.HttpContext);
            filterContext.ExceptionHandled = true;
            filterContext.Controller.TempData["Error"] =
                "Your session was refreshed. Please try that action again.";
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Dashboard", action = "Index" }));
        }
    }
}
