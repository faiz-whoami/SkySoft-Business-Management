using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace business_managment_system.Models
{
    public class ProductListViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public IList<Product> Items { get; set; }

        public ProductListViewModel()
        {
            Status = "active";
            Items = new List<Product>();
        }
    }

    public class ProductFormViewModel
    {
        public int ProductId { get; set; }

        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters.")]
        [Display(Name = "SKU")]
        public string Sku { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(150, ErrorMessage = "Product name cannot exceed 150 characters.")]
        [Display(Name = "Product name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(typeof(decimal), "0", "99999999.99", ErrorMessage = "Unit price cannot be negative.")]
        [Display(Name = "Unit price")]
        [DataType(DataType.Currency)]
        public decimal? UnitPrice { get; set; }
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public IList<ProductUsageRow> Usages { get; set; }

        public ProductDetailsViewModel()
        {
            Usages = new List<ProductUsageRow>();
        }
    }

    public class ProductUsageRow
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
