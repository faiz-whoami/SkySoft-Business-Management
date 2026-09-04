using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using business_managment_system.Helpers;

namespace business_managment_system.Models
{
    public class LookupOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PartyOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductOption
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class TransactionListViewModel
    {
        public string Search { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public IList<LookupOption> Types { get; set; }
        public IList<LookupOption> Statuses { get; set; }
        public IList<TransactionListRow> Items { get; set; }

        public TransactionListViewModel()
        {
            Types = new List<LookupOption>();
            Statuses = new List<LookupOption>();
            Items = new List<TransactionListRow>();
        }
    }

    public class TransactionListRow
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PartyName { get; set; }
        public string RecordedBy { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class TransactionDetailsViewModel
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PartyName { get; set; }
        public string RecordedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public IList<TransactionLineRow> Lines { get; set; }

        public TransactionDetailsViewModel()
        {
            Lines = new List<TransactionLineRow>();
        }

        public bool CanChangeStatus
        {
            get
            {
                return string.Equals(Status, TransactionNames.Pending, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public class TransactionLineRow
    {
        public int TransactionItemId { get; set; }
        public int? ProductId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class TransactionLineInput
    {
        public int? ProductId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class TransactionCreateViewModel
    {
        [Required(ErrorMessage = "Transaction type is required.")]
        [Display(Name = "Type")]
        public string TransactionTypeName { get; set; }

        [Required(ErrorMessage = "Party is required.")]
        [Display(Name = "Party")]
        public int? PartyId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? TransactionDate { get; set; }

        public string ItemsJson { get; set; }

        public string CatalogJson { get; set; }
    }

    public class TransactionCatalog
    {
        public IList<string> Types { get; set; }
        public IDictionary<string, IList<PartyOption>> Parties { get; set; }
        public IList<ProductOption> Products { get; set; }

        public TransactionCatalog()
        {
            Types = new List<string>();
            Parties = new Dictionary<string, IList<PartyOption>>();
            Products = new List<ProductOption>();
        }
    }
}
