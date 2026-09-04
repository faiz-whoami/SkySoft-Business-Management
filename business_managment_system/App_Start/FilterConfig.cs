using System.Web.Mvc;
using business_managment_system.Filters;

namespace business_managment_system
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AntiForgeryExceptionAttribute());
            filters.Add(new AuthorizeAttribute());
        }
    }
}
