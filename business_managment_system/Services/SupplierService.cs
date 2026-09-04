using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class SupplierService
    {
        private readonly SupplierRepository _suppliers;

        public SupplierService()
            : this(new SupplierRepository())
        {
        }

        public SupplierService(SupplierRepository suppliers)
        {
            _suppliers = suppliers;
        }

        public SupplierListViewModel GetList(string search, string status)
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

            return new SupplierListViewModel
            {
                Search = search,
                Status = filter,
                Items = _suppliers.Search(search, isActive)
            };
        }

        public Supplier Get(int id)
        {
            return _suppliers.GetById(id);
        }

        public SupplierDetailsViewModel GetDetails(int id)
        {
            var supplier = _suppliers.GetById(id);
            if (supplier == null)
            {
                return null;
            }

            return new SupplierDetailsViewModel
            {
                Supplier = supplier,
                Transactions = _suppliers.GetTransactions(id)
            };
        }

        public int Create(SupplierFormViewModel model)
        {
            return _suppliers.Create(ToEntity(model));
        }

        public void Update(SupplierFormViewModel model)
        {
            var existing = _suppliers.GetById(model.SupplierId);
            if (existing == null)
            {
                throw new InvalidOperationException("Supplier was not found.");
            }

            var entity = ToEntity(model);
            entity.SupplierId = model.SupplierId;
            _suppliers.Update(entity);
        }

        public void SetActive(int id, bool isActive)
        {
            if (_suppliers.GetById(id) == null)
            {
                throw new InvalidOperationException("Supplier was not found.");
            }

            _suppliers.SetActive(id, isActive);
        }

        public static SupplierFormViewModel ToForm(Supplier supplier)
        {
            return new SupplierFormViewModel
            {
                SupplierId = supplier.SupplierId,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address
            };
        }

        private static Supplier ToEntity(SupplierFormViewModel model)
        {
            return new Supplier
            {
                Name = (model.Name ?? string.Empty).Trim(),
                Email = TrimOrNull(model.Email),
                Phone = TrimOrNull(model.Phone),
                Address = TrimOrNull(model.Address)
            };
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
