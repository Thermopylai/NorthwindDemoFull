using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ViewModels
{
    public class OrderRows
    {
        [Display(Name = "Order")]
        [UIHint("Id_number")]
        public int OrderID { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }
                
        [UIHint("Amount_pcs")]
        public short Quantity { get; set; }

        [UIHint("Discount")]
        public float Discount { get; set; }
                
        [Display(Name = "Per Unit")]
        public string QuantityPerUnit { get; set; }

        [Display(Name = "In Stock")]
        [UIHint("Amount_pcs")]
        public Nullable<short> UnitsInStock { get; set; }
        
        [Display(Name = "Category")]
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}