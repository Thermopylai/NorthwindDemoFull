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
    public class TerritoryMetadata
    {
        [Key]
        [Required(ErrorMessage = "Territory ID is required!")]
        [RegularExpression(@"^[A-Za-z0-9]{1,20}$", ErrorMessage = "Territory ID must be 1 to 20 alphanumeric characters.")]
        [Display(Name = "ID")]
        public string TerritoryID { get; set; }

        [Required(ErrorMessage = "Territory name is required!")]
        [Display(Name = "Territory")]
        public string TerritoryDescription { get; set; }

        [Required(ErrorMessage = "Select region!")]
        [UIHint("Id_number")]
        [Display(Name = "Region")]
        public int RegionID { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(TerritoryMetadata))]
    public partial class Territory
    {
    }
}