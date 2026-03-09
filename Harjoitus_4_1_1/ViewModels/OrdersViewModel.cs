using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public class OrdersViewModel
    {
        public Order Order { get; set; }

        [Display(Name = "Items")]
        public IList<Order_Detail> OrderDetails { get; set; } = new List<Order_Detail>();
        public int OrderDetailsCount => OrderDetails?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}