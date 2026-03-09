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
    public class EmployeeMetadata
    {
        [UIHint("Id_number")]
        [Key]
        [Display(Name = "ID")]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Last name is required!")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "First name is required!")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Title is required!")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Title of courtesy is required!")]
        [Display(Name = "Salutation")]
        public string TitleOfCourtesy { get; set; }

        [Required(ErrorMessage = "Birth date is required!")]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> BirthDate { get; set; }

        [Required(ErrorMessage = "Hire date is required!")]
        [Display(Name = "Hire Date")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> HireDate { get; set; }

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

        [Required(ErrorMessage = "Home phone is required!")]
        [Display(Name = "Phone")]
        public string HomePhone { get; set; }

        public string Extension { get; set; } = string.Empty;

        [UIHint("Photo")]
        public byte[] Photo { get; set; } = null;

        public string Notes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose manager!")]
        [UIHint("Id_number")]
        [Display(Name = "Manager")]
        public Nullable<int> ReportsTo { get; set; }

        [Display(Name = "Photo Path")]
        public string PhotoPath { get; set; } = string.Empty;

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(EmployeeMetadata))]
    public partial class Employee { }
}