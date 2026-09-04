using System;
using System.Web.Mvc;
using business_managment_system.Helpers;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactions;

        public TransactionController()
            : this(new TransactionService())
        {
        }

        public TransactionController(TransactionService transactions)
        {
            _transactions = transactions;
        }

        public ActionResult Index(string search, string type, string status)
        {
            ViewBag.Title = "Transactions";
            return View(_transactions.GetList(search, type, status));
        }

        public ActionResult Details(int id)
        {
            var model = _transactions.GetDetails(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Transaction #" + model.TransactionId;
            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Title = "New transaction";
            return View(_transactions.NewCreateForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TransactionCreateViewModel model)
        {
            ViewBag.Title = "New transaction";
            if (!ModelState.IsValid)
            {
                model.CatalogJson = _transactions.BuildCatalogJson();
                return View(model);
            }

            var userIdObj = Session[SessionKeys.EndUserId];
            if (!(userIdObj is int))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var id = _transactions.Create(model, (int)userIdObj);
                TempData["Success"] = "Transaction #" + id + " was recorded as Pending.";
                return RedirectToAction("Details", new { id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.CatalogJson = _transactions.BuildCatalogJson();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetStatus(int id, string statusName)
        {
            try
            {
                _transactions.SetStatus(id, statusName);
                TempData["Success"] = "Transaction was marked " + statusName + ".";
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Transaction was not found.")
                {
                    return HttpNotFound();
                }

                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }
    }
}
