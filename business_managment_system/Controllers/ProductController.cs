using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Products";
            ViewBag.ModuleName = "Products";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
