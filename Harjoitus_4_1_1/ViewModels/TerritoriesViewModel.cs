using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Harjoitus_4_1_1.Models;

namespace Harjoitus_4_1_1.ViewModels
{
    public class TerritoriesViewModel
    {
        public Territory Territory { get; set; }

        public IList<Employee> Employees { get; set; } = new List<Employee>();
        public int EmployeesCount => Employees?.Count ?? 0;

        public string ReturnUrl { get; set; }
    }
}