using System.Collections.Generic;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public sealed class RegionsViewModel
    {
        public Region Region { get; set; }

        public IList<Territory> Territories { get; set; } = new List<Territory>();
        public int TerritoriesCount => Territories?.Count ?? 0;

        public IList<Shipper> Shippers { get; set; } = new List<Shipper>();
        public int ShippersCount => Shippers?.Count ?? 0;

        public IList<Employee> Employees { get; set; } = new List<Employee>();
        public int EmployeesCount => Employees?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}