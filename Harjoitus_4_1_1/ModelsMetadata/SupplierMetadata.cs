using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ModelsMetadata;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class SupplierMetadata
    {
        [Key]
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Supplier name is required!")]
        [Display(Name = "Supplier")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Contact name is required")]
        [Display(Name = "Contact")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Contact title is required!")]
        [Display(Name = "Title")]
        public string ContactTitle { get; set; }

        [Required(ErrorMessage = "Address is required!")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required!")]
        public string City { get; set; }
        
        public string Region { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required!")]
        [Display(Name = "Postal")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required!")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Phone number is required!")]
        public string Phone { get; set; }

        public string Fax { get; set; } = string.Empty;

        [Display(Name = "WWW")]
        public string HomePage { get; set; } = string.Empty;

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(SupplierMetadata))]
    public partial class Supplier
    {
    }
}