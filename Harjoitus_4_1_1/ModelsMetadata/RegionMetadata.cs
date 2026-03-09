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
    public class RegionMetadata
    {
        [Key]
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int RegionID { get; set; }

        [Required(ErrorMessage = "Region name is required!")]
        [Display(Name = "Region")]
        public string RegionDescription { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(RegionMetadata))]
    public partial class Region
    {
    }
}