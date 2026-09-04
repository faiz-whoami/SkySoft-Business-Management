using System.Web.Mvc;

namespace business_managment_system.Controllers
{
    public class TransactionController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Transactions";
            ViewBag.ModuleName = "Transactions";
            return View("~/Views/Shared/ComingSoon.cshtml");
        }
    }
}
