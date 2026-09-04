using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class ProductService
    {
        public const string DuplicateSkuMessage = "This SKU is already assigned to another product.";

        private readonly ProductRepository _products;

        public ProductService()
            : this(new ProductRepository())
        {
        }

        public ProductService(ProductRepository products)
        {
            _products = products;
        }

        public ProductListViewModel GetList(string search, string status)
        {
            var filter = (status ?? "active").Trim().ToLowerInvariant();
            bool? isActive = null;
            if (filter == "active")
            {
                isActive = true;
            }
            else if (filter == "inactive")
            {
                isActive = false;
            }
            else
            {
                filter = "all";
            }

            return new ProductListViewModel
            {
                Search = search,
                Status = filter,
                Items = _products.Search(search, isActive)
            };
        }

        public Product Get(int id)
        {
            return _products.GetById(id);
        }

        public ProductDetailsViewModel GetDetails(int id)
        {
            var product = _products.GetById(id);
            if (product == null)
            {
                return null;
            }

            return new ProductDetailsViewModel
            {
                Product = product,
                Usages = _products.GetUsages(id)
            };
        }

        public int Create(ProductFormViewModel model)
        {
            EnsureSkuAvailable(model.Sku, null);
            return _products.Create(ToEntity(model));
        }

        public void Update(ProductFormViewModel model)
        {
            var existing = _products.GetById(model.ProductId);
            if (existing == null)
            {
                throw new InvalidOperationException("Product was not found.");
            }

            EnsureSkuAvailable(model.Sku, model.ProductId);
            var entity = ToEntity(model);
            entity.ProductId = model.ProductId;
            _products.Update(entity);
        }

        public void SetActive(int id, bool isActive)
        {
            if (_products.GetById(id) == null)
            {
                throw new InvalidOperationException("Product was not found.");
            }

            _products.SetActive(id, isActive);
        }

        public static ProductFormViewModel ToForm(Product product)
        {
            return new ProductFormViewModel
            {
                ProductId = product.ProductId,
                Sku = product.Sku,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice
            };
        }

        private void EnsureSkuAvailable(string sku, int? excludeProductId)
        {
            if (_products.SkuExists(sku, excludeProductId))
            {
                throw new InvalidOperationException(DuplicateSkuMessage);
            }
        }

        private static Product ToEntity(ProductFormViewModel model)
        {
            return new Product
            {
                Sku = TrimOrNull(model.Sku),
                ProductName = (model.ProductName ?? string.Empty).Trim(),
                UnitPrice = model.UnitPrice.GetValueOrDefault()
            };
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
