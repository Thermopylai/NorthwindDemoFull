using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ModelsMetadata;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class ProductMetadata
    {
        [Key]
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required!")]
        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Select supplier!")]
        [UIHint("Id_number")]
        [Display(Name = "Supplier")]
        public Nullable<int> SupplierID { get; set; }

        [Required(ErrorMessage = "Select category!")]
        [UIHint("Id_number")]
        [Display(Name = "Category")]
        public Nullable<int> CategoryID { get; set; }

        [Display(Name = "Per Unit")]
        public string QuantityPerUnit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Unit price is required!")]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public Nullable<decimal> UnitPrice { get; set; }

        [Display(Name = "VAT")]
        [DataType(DataType.Currency)]
        public decimal TaxAmount { get; set; }

        [Display(Name = "w/VAT")]
        [DataType(DataType.Currency)]
        public decimal PriceWithTax { get; set; }

        [UIHint("Amount_pcs")]
        [Display(Name = "In Stock")]
        public Nullable<short> UnitsInStock { get; set; } = 0;
                
        [UIHint("Amount_pcs")]
        [Display(Name = "Ordered")]
        public Nullable<short> UnitsOnOrder { get; set; } = 0;
        
        [UIHint("Amount_pcs")]
        [Display(Name = "Reorder")]
        public Nullable<short> ReorderLevel { get; set; } = 0;

        [UIHint("IsRecycled")]
        [Display(Name = "Discont.")]
        public bool Discontinued { get; set; } = false;

        [Display(Name = "Image")]
        public string ImageLink { get; set; } = string.Empty;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        public decimal PriceWithTax => UnitPrice.HasValue ? UnitPrice.Value * 1.135m : 0m;
        public decimal TaxAmount => UnitPrice.HasValue ? UnitPrice.Value * 0.135m : 0m;
    }
}