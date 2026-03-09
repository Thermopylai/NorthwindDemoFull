using Harjoitus_4_1_1.ModelsMetadata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class CategoryMetadata
    {
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [UIHint("Amount_pcs")]
        [Display(Name = "Products")]
        public short ProductsInCategoryCount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        [UIHint("Photo")]
        public byte[] Picture { get; set; } = null;

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
        public short ProductsInCategoryCount
        {
            get
            {
                return (short)(this.Products != null ? this.Products.Count() : 0);
            }
        }
    }
}