using System;
using business_managment_system.Data;
using business_managment_system.Models;

namespace business_managment_system.Services
{
    public class CustomerService
    {
        private readonly CustomerRepository _customers;

        public CustomerService()
            : this(new CustomerRepository())
        {
        }

        public CustomerService(CustomerRepository customers)
        {
            _customers = customers;
        }

        public CustomerListViewModel GetList(string search, string status)
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

            return new CustomerListViewModel
            {
                Search = search,
                Status = filter,
                Items = _customers.Search(search, isActive)
            };
        }

        public Customer Get(int id)
        {
            return _customers.GetById(id);
        }

        public CustomerDetailsViewModel GetDetails(int id)
        {
            var customer = _customers.GetById(id);
            if (customer == null)
            {
                return null;
            }

            return new CustomerDetailsViewModel
            {
                Customer = customer,
                Transactions = _customers.GetTransactions(id)
            };
        }

        public int Create(CustomerFormViewModel model)
        {
            return _customers.Create(ToEntity(model));
        }

        public void Update(CustomerFormViewModel model)
        {
            var existing = _customers.GetById(model.CustomerId);
            if (existing == null)
            {
                throw new InvalidOperationException("Customer was not found.");
            }

            var entity = ToEntity(model);
            entity.CustomerId = model.CustomerId;
            _customers.Update(entity);
        }

        public void SetActive(int id, bool isActive)
        {
            if (_customers.GetById(id) == null)
            {
                throw new InvalidOperationException("Customer was not found.");
            }

            _customers.SetActive(id, isActive);
        }

        public static CustomerFormViewModel ToForm(Customer customer)
        {
            return new CustomerFormViewModel
            {
                CustomerId = customer.CustomerId,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address
            };
        }

        private static Customer ToEntity(CustomerFormViewModel model)
        {
            return new Customer
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
