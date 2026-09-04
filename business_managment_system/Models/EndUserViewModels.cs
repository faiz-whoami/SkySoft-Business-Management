using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace business_managment_system.Models
{
    public class EndUserListViewModel
    {
        public string Search { get; set; }
        public string Status { get; set; }
        public int CurrentUserId { get; set; }
        public IList<EndUser> Items { get; set; }

        public EndUserListViewModel()
        {
            Status = "active";
            Items = new List<EndUser>();
        }
    }

    public class EndUserFormViewModel : IValidatableObject
    {
        public int EndUserId { get; set; }
        public bool IsNew { get; set; }
        public bool IsSelf { get; set; }
        public string RoleName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Username may contain letters, numbers, dots, hyphens, and underscores only.")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "Role")]
        [Range(1, int.MaxValue, ErrorMessage = "Role is required.")]
        public int RoleId { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        public IList<SelectListItem> Roles { get; set; }

        public EndUserFormViewModel()
        {
            Roles = new List<SelectListItem>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsNew && string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("Password is required.", new[] { "Password" });
            }

            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 8)
            {
                yield return new ValidationResult("Password must be at least 8 characters.", new[] { "Password" });
            }
        }
    }

    public class EndUserDetailsViewModel
    {
        public EndUser User { get; set; }
        public bool IsSelf { get; set; }
        public IList<PartyTransactionRow> Transactions { get; set; }

        public EndUserDetailsViewModel()
        {
            Transactions = new List<PartyTransactionRow>();
        }
    }
}
