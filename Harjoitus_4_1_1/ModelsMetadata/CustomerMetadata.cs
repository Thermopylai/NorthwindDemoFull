using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ModelsMetadata;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class CustomerMetadata
    {
        [Key]
        [Required(ErrorMessage = "Customer ID is required!")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "Customer ID must be 1–5 characters.")]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Customer ID must be alphanumeric.")]
        [Display(Name = "ID")]
        public string CustomerID { get; set; }

        [Required(ErrorMessage = "Customer Name is required!")]
        [Display(Name = "Customer")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Contact Name is required!")]
        [Display(Name = "Contact")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Contact Title is required!")]
        [Display(Name = "Title")]
        public string ContactTitle { get; set; }

        [Required(ErrorMessage = "Address is required!")] 
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required!")]
        public string City { get; set; }

        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal Code is required!")]
        [Display(Name = "Postal")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required!")]       
        public string Country { get; set; }

        [Required(ErrorMessage = "Phone is required!")]
        public string Phone { get; set; }

        public string Fax { get; set; } = string.Empty;

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "Demographics")]
        public virtual ICollection<CustomerDemographic> CustomerDemographics { get; set; }
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(CustomerMetadata))]
    public partial class Customer
    {
    }
}