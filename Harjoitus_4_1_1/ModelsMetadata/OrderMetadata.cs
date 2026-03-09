using Harjoitus_4_1_1.ModelsMetadata;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ModelsMetadata
{
    public class OrderMetadata
    {
        [Key]
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Select customer!")]
        [Display(Name = "Customer")]
        public string CustomerID { get; set; }

        [Required(ErrorMessage = "Select employee!")]
        [UIHint("Id_number")]
        [Display(Name = "Employee")]
        public Nullable<int> EmployeeID { get; set; }

        [Required(ErrorMessage = "Order date is required!")]
        [DataType(DataType.Date)]
        [Display(Name = "Ordered")]
        public Nullable<System.DateTime> OrderDate { get; set; }

        [Required(ErrorMessage = "Required date is required!")]
        [DataType(DataType.Date)]
        [Display(Name = "Required")]
        public Nullable<System.DateTime> RequiredDate { get; set; }


        [DataType(DataType.Date)]
        [Display(Name = "Shipped")]
        public Nullable<System.DateTime> ShippedDate { get; set; }

        [Required(ErrorMessage = "Select shipper!")]
        [UIHint("Id_number")]
        [Display(Name = "Shipper")]
        public Nullable<int> ShipVia { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "VAT")]
        [DataType(DataType.Currency)]
        public decimal TotalTax { get; set; }

        [Display(Name = "w/VAT")]
        [DataType(DataType.Currency)]
        public decimal VatTotal { get; set; }

        [Required(ErrorMessage = "Freight is required!")]
        [DataType(DataType.Currency)]
        public Nullable<decimal> Freight { get; set; }

        [Display(Name = "Final")]
        [DataType(DataType.Currency)]
        public decimal FinalPrice { get; set; }

        [Required(ErrorMessage = "Customer name is required!")]
        [Display(Name = "Name")]
        public string ShipName { get; set; }

        [Required(ErrorMessage = "Address is required!")]
        [Display(Name = "Address")]
        public string ShipAddress { get; set; }

        [Required(ErrorMessage = "City is required!")]
        [Display(Name = "City")]
        public string ShipCity { get; set; }

        [Display(Name = "Region")]
        public string ShipRegion { get; set; }

        [Required(ErrorMessage = "Postal code is required!")]
        [Display(Name = "Postal")]
        public string ShipPostalCode { get; set; }

        [Required(ErrorMessage = "Country is required!")]
        [Display(Name = "Country")]
        public string ShipCountry { get; set; }

        [UIHint("IsRecycled")]
        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}

namespace Harjoitus_4_1_1.Models
{
    [MetadataType(typeof(OrderMetadata))]
    public partial class Order
    {
        public decimal TotalAmount
        {
            get
            {
                decimal total = 0;
                foreach (var detail in this.Order_Details)
                {
                    total += detail.TotalPrice;
                }
                return total;
            }
        }

        public decimal TotalTax => TotalAmount * 0.135m;

        public decimal VatTotal => TotalAmount * 1.135m;

        public decimal FinalPrice => VatTotal + (Freight ?? 0m);

    }
}