using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace business_managment_system.Models
{
    public class SupplierListViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public IList<Supplier> Items { get; set; }

        public SupplierListViewModel()
        {
            Status = "active";
            Items = new List<Supplier>();
        }
    }

    public class SupplierFormViewModel
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Supplier name")]
        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters.")]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Address")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
    }

    public class SupplierDetailsViewModel
    {
        public Supplier Supplier { get; set; }
        public IList<PartyTransactionRow> Transactions { get; set; }

        public SupplierDetailsViewModel()
        {
            Transactions = new List<PartyTransactionRow>();
        }
    }
}
