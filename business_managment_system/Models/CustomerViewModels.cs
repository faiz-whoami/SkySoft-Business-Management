using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace business_managment_system.Models
{
    public class CustomerListViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public IList<Customer> Items { get; set; }

        public CustomerListViewModel()
        {
            Status = "active";
            Items = new List<Customer>();
        }
    }

    public class CustomerFormViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        [Display(Name = "Customer name")]
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

    public class CustomerDetailsViewModel
    {
        public Customer Customer { get; set; }
        public IList<CustomerTransactionRow> Transactions { get; set; }

        public CustomerDetailsViewModel()
        {
            Transactions = new List<CustomerTransactionRow>();
        }
    }

    public class CustomerTransactionRow
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
