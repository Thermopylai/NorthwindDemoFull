using Harjoitus_4_1_1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Harjoitus_4_1_1.ViewModels
{
    public class EmployeesViewModel
    {
        public Employee Employee { get; set; }

        public IList<Order> Orders { get; set; } = new List<Order>();
        public int OrdersCount => Orders?.Count ?? 0;

        public IList<Product> Products { get; set; } = new List<Product>();
        public int ProductsCount => Products?.Count ?? 0;

        public IList<Order_Detail> OrderDetails { get; set; } = new List<Order_Detail>();
        public int OrderDetailsCount => OrderDetails?.Count ?? 0;

        public IList<EmployeeServedProduct> EmployeeServedProducts { get; set; } = new List<EmployeeServedProduct>();

        public IList<EmployeeServedProduct> Calculate()
        {
            foreach (var detail in OrderDetails)
            {
                var detailProduct = Products.FirstOrDefault(p => p.ProductID == detail.ProductID);
                if (detailProduct != null)
                {
                    var existingItem = EmployeeServedProducts.FirstOrDefault(cop => cop.ProductID == detailProduct.ProductID);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += detail.Quantity;
                        existingItem.TotalAmount += detail.UnitPrice * detail.Quantity * (1 - (decimal)detail.Discount);
                    }
                    else
                    {
                        EmployeeServedProducts.Add(new EmployeeServedProduct
                        {
                            ProductID = detailProduct.ProductID,
                            ProductName = detailProduct.ProductName,
                            CategoryName = detailProduct.Category?.CategoryName,
                            QuantityPerUnit = detailProduct.QuantityPerUnit,
                            UnitPrice = detail.UnitPrice,
                            Quantity = detail.Quantity,
                            TotalAmount = detail.UnitPrice * detail.Quantity * (1 - (decimal)detail.Discount),
                            Discontinued = detailProduct.Discontinued
                        });
                    }
                }
            }
            EmployeeServedProducts = EmployeeServedProducts.OrderBy(cop => cop.ProductID).ToList();
            return EmployeeServedProducts;
        }

        public IList<Territory> Territories { get; set; } = new List<Territory>();
        public int TerritoriesCount => Territories?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }

    public class EmployeeServedProduct
    {
        [UIHint("Id_number")]
        [Display(Name = "ID")]
        public int ProductID { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Per unit")]
        public string QuantityPerUnit { get; set; }

        [Display(Name = "Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Amount")]
        [UIHint("Amount_pcs")]
        public short Quantity { get; set; }

        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "VAT")]
        [DataType(DataType.Currency)]
        public decimal TaxAmount => TotalAmount * 0.135m;

        [Display(Name = "w/VAT")]
        [DataType(DataType.Currency)]
        public decimal VatPrice => TotalAmount * 1.135m;

        [UIHint("IsRecycled")]
        [Display(Name = "Discont.")]
        public bool Discontinued { get; set; } = false;
    }
}