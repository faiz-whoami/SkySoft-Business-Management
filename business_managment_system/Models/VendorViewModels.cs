using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace business_managment_system.Models
{
    public class VendorListViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public IList<Vendor> Items { get; set; }

        public VendorListViewModel()
        {
            Status = "active";
            Items = new List<Vendor>();
        }
    }

    public class VendorFormViewModel
    {
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Vendor name")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Service type cannot exceed 100 characters.")]
        [Display(Name = "Service type")]
        public string ServiceType { get; set; }

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

    public class VendorDetailsViewModel
    {
        public Vendor Vendor { get; set; }
        public IList<PartyTransactionRow> Transactions { get; set; }

        public VendorDetailsViewModel()
        {
            Transactions = new List<PartyTransactionRow>();
        }
    }
}
