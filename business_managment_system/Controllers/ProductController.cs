using System;
using System.Web.Mvc;
using business_managment_system.Filters;
using business_managment_system.Models;
using business_managment_system.Services;

namespace business_managment_system.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _products;

        public ProductController()
            : this(new ProductService())
        {
        }

        public ProductController(ProductService products)
        {
            _products = products;
        }

        public ActionResult Index(string search, string status)
        {
            ViewBag.Title = "Products";
            var model = _products.GetList(search, status);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _products.GetDetails(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = model.Product.ProductName;
            return View(model);
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.Title = "New product";
            return View(new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Create(ProductFormViewModel model)
        {
            ViewBag.Title = "New product";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _products.Create(model);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == ProductService.DuplicateSkuMessage)
                {
                    ModelState.AddModelError("Sku", ex.Message);
                    return View(model);
                }

                throw;
            }

            TempData["Success"] = "Product was created.";
            return RedirectToAction("Index");
        }

        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var product = _products.Get(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.Title = "Edit product";
            return View(ProductService.ToForm(product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult Edit(ProductFormViewModel model)
        {
            ViewBag.Title = "Edit product";
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _products.Update(model);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == ProductService.DuplicateSkuMessage)
                {
                    ModelState.AddModelError("Sku", ex.Message);
                    return View(model);
                }

                return HttpNotFound();
            }

            TempData["Success"] = "Product was updated.";
            return RedirectToAction("Details", new { id = model.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AppAuthorize(Roles = "Admin")]
        public ActionResult ToggleStatus(int id, string search, string status)
        {
            var product = _products.Get(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            var nowActive = !product.IsActive;
            _products.SetActive(id, nowActive);

            if (nowActive)
            {
                TempData["Success"] = "Product was reactivated and is available on new transactions.";
                return RedirectToAction("Index", new { search, status = "active" });
            }

            TempData["Success"] = "Product was deactivated. Open Status → Inactive to reactivate it later.";
            return RedirectToAction("Index", new { search, status = "inactive" });
        }
    }
}
