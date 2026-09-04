using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Reports";
            ViewBag.ModuleName = "Reports";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
