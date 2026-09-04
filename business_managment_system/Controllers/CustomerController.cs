using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class CustomerController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Customers";
            ViewBag.ModuleName = "Customers";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
