using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class VendorController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Vendors";
            ViewBag.ModuleName = "Vendors";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
