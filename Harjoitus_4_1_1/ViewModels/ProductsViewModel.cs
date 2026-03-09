using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public class ProductsViewModel
    {
        public Product Product { get; set; }

        public IList<Order> Orders { get; set; } = new List<Order>();
        public int OrdersCount => Orders?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}