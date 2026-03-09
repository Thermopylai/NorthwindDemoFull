using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public class ShippersViewModel
    {
        public Shipper Shipper { get; set; }

        public IList<Order> Orders { get; set; } = new List<Order>();
        public int OrdersCount => Orders?.Count ?? 0;

        public IList<Territory> Territories { get; set; } = new List<Territory>();
        public int TerritoriesCount => Territories?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}