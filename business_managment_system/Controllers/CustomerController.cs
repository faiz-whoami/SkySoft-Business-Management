using System;
using System.Web.Mvc;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerService _customers;

        public CustomerController()
            : this(new CustomerService())
        {
        }

        public CustomerController(CustomerService customers)
        {
            _customers = customers;
        }

        public ActionResult Index(string search, string status)
        {
            ViewBag.Title = "Customers";
            var model = _customers.GetList(search, status);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _customers.GetDetails(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = model.Customer.Name;
            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "New customer";
            return View(new CustomerFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerFormViewModel model)
        {
            ViewBag.Title = "New customer";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _customers.Create(model);
            TempData["Success"] = "Customer was created.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var customer = _customers.Get(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit customer";
            return View(CustomerService.ToForm(customer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerFormViewModel model)
        {
            ViewBag.Title = "Edit customer";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _customers.Update(model);
            }
            catch (InvalidOperationException)
            {
                return HttpNotFound();
            }

            TempData["Success"] = "Customer was updated.";
            return RedirectToAction("Details", new { id = model.CustomerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, string search, string status)
        {
            var customer = _customers.Get(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var nowActive = !customer.IsActive;
            _customers.SetActive(id, nowActive);

            if (nowActive)
            {
                TempData["Success"] = "Customer was reactivated and is available for new transactions.";
                return RedirectToAction("Index", new { search, status = "active" });
            }

            TempData["Success"] = "Customer was deactivated. Open Status → Inactive to reactivate them later.";
            return RedirectToAction("Index", new { search, status = "inactive" });
        }
    }
}
