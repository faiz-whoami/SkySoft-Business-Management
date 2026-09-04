using System;
using System.Web.Mvc;
using business_managment_system.Data;
using business_managment_system.Helpers;

namespace business_managment_system.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardRepository _dashboard;

        public DashboardController()
            : this(new DashboardRepository())
        {
        }

        public DashboardController(DashboardRepository dashboard)
        {
            _dashboard = dashboard;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            ViewBag.UserFirstName = GetFirstName();

            try
            {
                return View(_dashboard.GetDashboard());
            }
            catch (Exception)
            {
                TempData["Error"] = "The dashboard could not load data from SQL Server. Confirm the SkySoftDB connection and try again.";
                return View(new business_managment_system.Models.DashboardViewModel
                {
                    MonthLabel = DateTime.Today.ToString("MMMM yyyy")
                });
            }
        }

        private string GetFirstName()
        {
            var fullName = Session[SessionKeys.FullName] as string;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return User.Identity.Name;
            }

            var space = fullName.IndexOf(' ');
            return space > 0 ? fullName.Substring(0, space) : fullName;
        }
    }
}
