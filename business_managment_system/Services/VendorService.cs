using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class VendorService
    {
        private readonly VendorRepository _vendors;

        public VendorService()
            : this(new VendorRepository())
        {
        }

        public VendorService(VendorRepository vendors)
        {
            _vendors = vendors;
        }

        public VendorListViewModel GetList(string search, string status)
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

            return new VendorListViewModel
            {
                Search = search,
                Status = filter,
                Items = _vendors.Search(search, isActive)
            };
        }

        public Vendor Get(int id)
        {
            return _vendors.GetById(id);
        }

        public VendorDetailsViewModel GetDetails(int id)
        {
            var vendor = _vendors.GetById(id);
            if (vendor == null)
            {
                return null;
            }

            return new VendorDetailsViewModel
            {
                Vendor = vendor,
                Transactions = _vendors.GetTransactions(id)
            };
        }

        public int Create(VendorFormViewModel model)
        {
            return _vendors.Create(ToEntity(model));
        }

        public void Update(VendorFormViewModel model)
        {
            var existing = _vendors.GetById(model.VendorId);
            if (existing == null)
            {
                throw new InvalidOperationException("Vendor was not found.");
            }

            var entity = ToEntity(model);
            entity.VendorId = model.VendorId;
            _vendors.Update(entity);
        }

        public void SetActive(int id, bool isActive)
        {
            if (_vendors.GetById(id) == null)
            {
                throw new InvalidOperationException("Vendor was not found.");
            }

            _vendors.SetActive(id, isActive);
        }

        public static VendorFormViewModel ToForm(Vendor vendor)
        {
            return new VendorFormViewModel
            {
                VendorId = vendor.VendorId,
                Name = vendor.Name,
                ServiceType = vendor.ServiceType,
                Email = vendor.Email,
                Phone = vendor.Phone,
                Address = vendor.Address
            };
        }

        private static Vendor ToEntity(VendorFormViewModel model)
        {
            return new Vendor
            {
                Name = (model.Name ?? string.Empty).Trim(),
                ServiceType = TrimOrNull(model.ServiceType),
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
