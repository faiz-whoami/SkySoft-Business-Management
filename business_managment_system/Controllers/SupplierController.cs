using System;
using System.Web.Mvc;
using business_managment_system.Filters;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class SupplierController : Controller
    {
        private readonly SupplierService _suppliers;

        public SupplierController()
            : this(new SupplierService())
        {
        }

        public SupplierController(SupplierService suppliers)
        {
            _suppliers = suppliers;
        }

        public ActionResult Index(string search, string status)
        {
            ViewBag.Title = "Suppliers";
            var model = _suppliers.GetList(search, status);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _suppliers.GetDetails(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = model.Supplier.Name;
            return View(model);
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Title = "New supplier";
            return View(new SupplierFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create(SupplierFormViewModel model)
        {
            ViewBag.Title = "New supplier";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _suppliers.Create(model);
            TempData["Success"] = "Supplier was created.";
            return RedirectToAction("Index");
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var supplier = _suppliers.Get(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit supplier";
            return View(SupplierService.ToForm(supplier));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(SupplierFormViewModel model)
        {
            ViewBag.Title = "Edit supplier";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _suppliers.Update(model);
            }
            catch (InvalidOperationException)
            {
                return HttpNotFound();
            }

            TempData["Success"] = "Supplier was updated.";
            return RedirectToAction("Details", new { id = model.SupplierId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult ToggleStatus(int id, string search, string status)
        {
            var supplier = _suppliers.Get(id);
            if (supplier == null)
            {
                return HttpNotFound();
            }

            var nowActive = !supplier.IsActive;
            _suppliers.SetActive(id, nowActive);

            if (nowActive)
            {
                TempData["Success"] = "Supplier was reactivated and is available for new purchases.";
                return RedirectToAction("Index", new { search, status = "active" });
            }

            TempData["Success"] = "Supplier was deactivated. Open Status → Inactive to reactivate them later.";
            return RedirectToAction("Index", new { search, status = "inactive" });
        }
    }
}
