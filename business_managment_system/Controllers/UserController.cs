using System.Web.Mvc;
using business_managment_system.Filters;

namespace business_managment_system.Controllers
{
    [AppAuthorize(Roles = "Admin")]
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "End Users";
            ViewBag.ModuleName = "End Users";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
