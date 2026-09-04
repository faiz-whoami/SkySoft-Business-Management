using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class SupplierController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Suppliers";
            ViewBag.ModuleName = "Suppliers";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
