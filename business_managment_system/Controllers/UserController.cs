using System.Web.Mvc;

namespace business_managment_system.Controllers
{
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
