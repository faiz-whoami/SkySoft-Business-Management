using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using business_managment_system.Data;
using business_managment_system.Helpers;
using business_managment_system.Models;
using Newtonsoft.Json;

namespace business_managment_system.Services
{
    public class TransactionService
    {
        private readonly TransactionRepository _transactions;

        public TransactionService()
            : this(new TransactionRepository())
        {
        }

        public TransactionService(TransactionRepository transactions)
        {
            _transactions = transactions;
        }

        public TransactionListViewModel GetList(string search, string type, string status)
        {
            var typeName = NormalizeFilter(type);
            var statusName = NormalizeFilter(status);

            return new TransactionListViewModel
            {
                Search = search,
                Type = typeName ?? string.Empty,
                Status = statusName ?? string.Empty,
                Types = _transactions.GetTypes(),
                Statuses = _transactions.GetStatuses(),
                Items = _transactions.Search(search, typeName, statusName)
            };
        }

        public TransactionDetailsViewModel GetDetails(int id)
        {
            return _transactions.GetDetails(id);
        }

        public TransactionCreateViewModel NewCreateForm()
        {
            return new TransactionCreateViewModel
            {
                TransactionTypeName = TransactionNames.Sale,
                TransactionDate = DateTime.Today,
                CatalogJson = BuildCatalogJson()
            };
        }

        public string BuildCatalogJson()
        {
            var catalog = new TransactionCatalog
            {
                Types = _transactions.GetTypes().Select(item => item.Name).ToList(),
                Parties =
                {
                    { TransactionNames.Sale, _transactions.GetActiveCustomers() },
                    { TransactionNames.Purchase, _transactions.GetActiveSuppliers() },
                    { TransactionNames.Service, _transactions.GetActiveVendors() }
                },
                Products = _transactions.GetActiveProducts()
            };

            return JsonConvert.SerializeObject(catalog);
        }

        public int Create(TransactionCreateViewModel model, int createdByUserId)
        {
            var typeId = RequireTypeId(model.TransactionTypeName);
            var partyId = model.PartyId.GetValueOrDefault();
            if (partyId <= 0)
            {
                throw new InvalidOperationException("Select a party for this transaction.");
            }

            int? customerId = null;
            int? supplierId = null;
            int? vendorId = null;
            AssignParty(model.TransactionTypeName, partyId, ref customerId, ref supplierId, ref vendorId);

            var lines = ParseLines(model.ItemsJson);
            ValidateLines(lines);

            var itemsJson = JsonConvert.SerializeObject(lines.Select(line => new
            {
                line.ProductId,
                line.Description,
                line.Quantity,
                line.UnitPrice
            }));

            try
            {
                return _transactions.CreateWithItems(
                    typeId,
                    customerId,
                    supplierId,
                    vendorId,
                    createdByUserId,
                    model.TransactionDate,
                    itemsJson);
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public void SetStatus(int id, string statusName)
        {
            var existing = _transactions.GetDetails(id);
            if (existing == null)
            {
                throw new InvalidOperationException("Transaction was not found.");
            }

            if (!string.Equals(existing.Status, TransactionNames.Pending, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only pending transactions can change status.");
            }

            if (!string.Equals(statusName, TransactionNames.Completed, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(statusName, TransactionNames.Cancelled, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Status must be Completed or Cancelled.");
            }

            var statusId = _transactions.GetStatusIdByName(statusName);
            if (!statusId.HasValue)
            {
                throw new InvalidOperationException("Transaction status '" + statusName + "' was not found.");
            }

            _transactions.SetStatus(id, statusId.Value);
        }

        private int RequireTypeId(string typeName)
        {
            if (!TransactionNames.IsKnownType(typeName))
            {
                throw new InvalidOperationException("Transaction type is not recognised.");
            }

            var typeId = _transactions.GetTypeIdByName(typeName.Trim());
            if (!typeId.HasValue)
            {
                throw new InvalidOperationException("Transaction type '" + typeName + "' was not found.");
            }

            return typeId.Value;
        }

        private void AssignParty(string typeName, int partyId, ref int? customerId, ref int? supplierId, ref int? vendorId)
        {
            if (string.Equals(typeName, TransactionNames.Sale, StringComparison.OrdinalIgnoreCase))
            {
                if (!_transactions.CustomerIsActive(partyId))
                {
                    throw new InvalidOperationException("Select an active customer.");
                }

                customerId = partyId;
                return;
            }

            if (string.Equals(typeName, TransactionNames.Purchase, StringComparison.OrdinalIgnoreCase))
            {
                if (!_transactions.SupplierIsActive(partyId))
                {
                    throw new InvalidOperationException("Select an active supplier.");
                }

                supplierId = partyId;
                return;
            }

            if (string.Equals(typeName, TransactionNames.Service, StringComparison.OrdinalIgnoreCase))
            {
                if (!_transactions.VendorIsActive(partyId))
                {
                    throw new InvalidOperationException("Select an active vendor.");
                }

                vendorId = partyId;
                return;
            }

            throw new InvalidOperationException("Transaction type is not recognised.");
        }

        private static IList<TransactionLineInput> ParseLines(string itemsJson)
        {
            if (string.IsNullOrWhiteSpace(itemsJson))
            {
                return new List<TransactionLineInput>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<TransactionLineInput>>(itemsJson)
                    ?? new List<TransactionLineInput>();
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("Line items could not be read. Add at least one valid line.");
            }
        }

        private static void ValidateLines(IList<TransactionLineInput> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new InvalidOperationException("Add at least one line item.");
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line.Description))
                {
                    throw new InvalidOperationException("Each line needs a description.");
                }

                if (line.Description.Length > 200)
                {
                    throw new InvalidOperationException("Line descriptions cannot exceed 200 characters.");
                }

                if (line.Quantity <= 0)
                {
                    throw new InvalidOperationException("Quantity must be greater than zero.");
                }

                if (line.UnitPrice < 0)
                {
                    throw new InvalidOperationException("Unit price cannot be negative.");
                }

                line.Description = line.Description.Trim();
                if (line.ProductId.HasValue && line.ProductId.Value <= 0)
                {
                    line.ProductId = null;
                }
            }
        }

        private static string NormalizeFilter(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
