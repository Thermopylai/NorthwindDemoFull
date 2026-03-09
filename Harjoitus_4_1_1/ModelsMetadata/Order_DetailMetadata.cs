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
    public class Order_DetailMetadata
    {
        [Key]
        [Required(ErrorMessage = "Select order!")]
        [UIHint("Id_number")]
        [Display(Name = "Order")]
        public int OrderID { get; set; }

        [Key]
        [Required(ErrorMessage = "Select product!")]
        [UIHint("Id_number")]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; } = 0m;

        [Required(ErrorMessage = "Quantity is required!")]
        [UIHint("Amount_pcs")]
        public short Quantity { get; set; } = 1;

        [Display(Name = "Discount")]
        [UIHint("Discount")]
        public float Discount { get; set; } = 0f;
                
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [Display(Name = "VAT")]
        [DataType(DataType.Currency)]
        public decimal TaxAmount { get; set; }

        [Display(Name = "w/VAT")]
        [DataType(DataType.Currency)]
        public decimal VatPrice { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(Order_DetailMetadata))]
    public partial class Order_Detail
    {
        public string CustomerID 
        {
            get
            {
                return this.Order.CustomerID;
            }
        }

        public decimal TotalPrice => UnitPrice * Quantity * (decimal)(1 - Discount);
        public decimal VatPrice => TotalPrice * 1.135m;
        public decimal TaxAmount => TotalPrice * 0.135m;
    }
}