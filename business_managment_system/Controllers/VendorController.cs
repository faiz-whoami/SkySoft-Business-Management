using System;
using System.Web.Mvc;
using business_managment_system.Filters;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class VendorController : Controller
    {
        private readonly VendorService _vendors;

        public VendorController()
            : this(new VendorService())
        {
        }

        public VendorController(VendorService vendors)
        {
            _vendors = vendors;
        }

        public ActionResult Index(string search, string status)
        {
            ViewBag.Title = "Vendors";
            var model = _vendors.GetList(search, status);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _vendors.GetDetails(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = model.Vendor.Name;
            return View(model);
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Title = "New vendor";
            return View(new VendorFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create(VendorFormViewModel model)
        {
            ViewBag.Title = "New vendor";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _vendors.Create(model);
            TempData["Success"] = "Vendor was created.";
            return RedirectToAction("Index");
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var vendor = _vendors.Get(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit vendor";
            return View(VendorService.ToForm(vendor));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(VendorFormViewModel model)
        {
            ViewBag.Title = "Edit vendor";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _vendors.Update(model);
            }
            catch (InvalidOperationException)
            {
                return HttpNotFound();
            }

            TempData["Success"] = "Vendor was updated.";
            return RedirectToAction("Details", new { id = model.VendorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult ToggleStatus(int id, string search, string status)
        {
            var vendor = _vendors.Get(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            var nowActive = !vendor.IsActive;
            _vendors.SetActive(id, nowActive);

            if (nowActive)
            {
                TempData["Success"] = "Vendor was reactivated and is available for new service transactions.";
                return RedirectToAction("Index", new { search, status = "active" });
            }

            TempData["Success"] = "Vendor was deactivated. Open Status → Inactive to reactivate them later.";
            return RedirectToAction("Index", new { search, status = "inactive" });
        }
    }
}
