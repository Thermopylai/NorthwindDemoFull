using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ModelsMetadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class CustomerDemographicMetadata
    {
        [Key]
        [Required(ErrorMessage = "Customer type is required!")]
        [RegularExpression(@"^[A-Za-z0-9]{10}$", ErrorMessage = "Customer type must be exactly 10 alphanumeric characters.")]
        [Display(Name = "ID")]
        public string CustomerTypeID { get; set; }

        [Required(ErrorMessage = "Customer description is required!")]
        [Display(Name = "Description")]
        public string CustomerDesc { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(CustomerDemographicMetadata))]
    public partial class CustomerDemographic
    {
    }
}