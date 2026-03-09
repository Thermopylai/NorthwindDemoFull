using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public class CategoriesViewModel
    {
        public Category Category { get; set; }

        public IList<Product> Products { get; set; } = new List<Product>();
        public int ProductsCount => Products?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}