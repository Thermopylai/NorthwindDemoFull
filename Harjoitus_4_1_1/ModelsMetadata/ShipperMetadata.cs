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
    public class ShipperMetadata
    {
        [Key]
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int ShipperID { get; set; }

        [Required(ErrorMessage = "Shipper name is required!")]
        [Display(Name = "Shipper")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Phone number is required!")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Select region!")]
        [UIHint("Id_number")]
        [Display(Name = "Region")]
        public Nullable<int> RegionID { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(ShipperMetadata))]
    public partial class Shipper
    {
    }
}