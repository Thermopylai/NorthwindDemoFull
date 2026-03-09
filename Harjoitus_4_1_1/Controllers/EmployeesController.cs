using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ViewModels;

namespace Harjoitus_4_1_1.Controllers
{
    public class EmployeesController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        private void PopulateTerritoriesSelectList(Employee employee = null)
        {
            var allTerritories = db.Territories
                .OrderBy(t => t.TerritoryDescription)
                .Select(t => new
                {
                    t.TerritoryID,
                    TerritoryDescription = t.TerritoryDescription.Trim()
                })
                .ToList();

            var selected = new List<string>();

            if (employee != null)
            {
                selected = employee.Territories
                    .Select(t => t.TerritoryID)
                    .ToList();
            }

            ViewBag.TerritoryIDs = new MultiSelectList(
                allTerritories,
                "TerritoryID",
                "TerritoryDescription",
                selected);
        }

        // GET: Employees
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var employees = db.Employees
                .Where(e => !e.IsDeleted)
                .Include(e => e.Employee1)
                .OrderBy(e => e.EmployeeID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await employees.ToListAsync());
        }

        // GET: Employees/RecycleBin (deleted employees)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var employees = db.Employees
                .Where(e => e.IsDeleted)
                .Include(e => e.Employee1)
                .OrderBy(e => e.EmployeeID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           
            var employee = await db.Employees
                .Include(e => e.Territories)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
            if (employee == null)
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.EmployeeID == id && !od.Order.IsDeleted);

            var vm = new EmployeesViewModel 
            { 
                Employee = employee,
                Territories = (employee.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Orders = (employee.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (employee.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .SelectMany(o => o.Order_Details ?? new List<Order_Detail>())
                    .Where(od => od.Product != null)
                    .Select(od => od.Product)
                    .Distinct()
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                OrderDetails = await orderDetails
                    .Where(od => !od.IsDeleted)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .ToListAsync(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            vm.EmployeeServedProducts = vm.Calculate();

            return View(vm);
        }

        // GET: Employees/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.ReportsTo = new SelectList(db.Employees, "EmployeeID", "LastName");
            PopulateTerritoriesSelectList();
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "EmployeeID,LastName,FirstName,Title,TitleOfCourtesy,BirthDate,HireDate,Address,City,Region,PostalCode,Country,HomePhone,Extension,Notes,ReportsTo,PhotoPath")] 
            Employee employee,
            string[] TerritoryID,
            string returnUrl = null)
        {
            employee.IsDeleted = false;
            if (TerritoryID != null)
            {
                var territories = await db.Territories
                    .Where(t => TerritoryID.Contains(t.TerritoryID))
                    .ToListAsync();
                foreach (var t in territories)
                    employee.Territories.Add(t);
            }
            if (ModelState.IsValid)
            {
                db.Employees.Add(employee);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.ReportsTo = new SelectList(db.Employees, "EmployeeID", "LastName", employee.ReportsTo);
            PopulateTerritoriesSelectList(employee);
            return View(employee);
        }

        // GET: Employees/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          
            var employee = await db.Employees
                .Include(e => e.Territories)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
            if (employee == null)
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.EmployeeID == id && !od.Order.IsDeleted);

            var vm = new EmployeesViewModel
            {
                Employee = employee,
                Territories = (employee.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Orders = (employee.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (employee.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .SelectMany(o => o.Order_Details ?? new List<Order_Detail>())
                    .Where(od => od.Product != null)
                    .Select(od => od.Product)
                    .Distinct()
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                OrderDetails = await orderDetails
                    .Where(od => !od.IsDeleted)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .ToListAsync(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            vm.EmployeeServedProducts = vm.Calculate();

            ViewBag.ReportsTo = new SelectList(db.Employees, "EmployeeID", "LastName", employee.ReportsTo);

            ModelState.Remove("TerritoryID");
            PopulateTerritoriesSelectList(employee);
            return View(vm);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            EmployeesViewModel vm,
            string[] TerritoryID,
            string returnUrl = null)
        {
            if (vm == null || vm.Employee == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Employees
                .Include(e => e.Territories)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.EmployeeID == vm.Employee.EmployeeID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Employee", new[]
            {
                "LastName",
                "FirstName",
                "Title",
                "TitleOfCourtesy",
                "BirthDate",
                "HireDate",
                "Address",
                "City",
                "Region",
                "PostalCode",
                "Country",
                "HomePhone",
                "Extension",
                "Notes",
                "ReportsTo",
                "PhotoPath"
            }))
            {
                // Only update territories if the field was actually posted.
                // This prevents accidental clearing when no value comes through.
                if (Request.Form.AllKeys.Contains("TerritoryID"))
                {
                    var selected = TerritoryID ?? new string[0];

                    itemToUpdate.Territories.Clear();

                    if (selected.Length > 0)
                    {
                        var newTerritories = await db.Territories
                            .Where(t => selected.Contains(t.TerritoryID))
                            .ToListAsync();
                        foreach (var t in newTerritories)
                            itemToUpdate.Territories.Add(t);
                    }
                }
                if (ModelState.IsValid)
                {
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    var fallbackUrl = vm.ReturnUrl ?? Url.Action(nameof(Index));
                    return Redirect(fallbackUrl);
                }
            }

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.EmployeeID == itemToUpdate.EmployeeID && !od.Order.IsDeleted);

            vm.Employee = itemToUpdate;
            vm.Territories = (itemToUpdate.Territories ?? new List<Territory>())
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.TerritoryID)
                .ToList();
            vm.Orders = (itemToUpdate.Orders ?? new List<Order>())
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.OrderID)
                .ToList();
            vm.Products = (itemToUpdate.Orders ?? new List<Order>())
                .Where(o => !o.IsDeleted)
                .SelectMany(o => o.Order_Details ?? new List<Order_Detail>())
                .Where(od => od.Product != null)
                .Select(od => od.Product)
                .Distinct()
                .OrderBy(p => p.ProductID)
                .ToList();
            vm.OrderDetails = await orderDetails
                .Where(od => !od.IsDeleted)
                .OrderBy(od => od.OrderID)
                .ThenBy(od => od.ProductID)
                .ToListAsync();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            vm.EmployeeServedProducts = vm.Calculate();

            ViewBag.ReturnUrl = vm.ReturnUrl;
            ViewBag.ReportsTo = new SelectList(db.Employees, "EmployeeID", "LastName", itemToUpdate.ReportsTo);

            ModelState.Remove("TerritoryID");
            PopulateTerritoriesSelectList(itemToUpdate);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Employees/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee == null) 
                return HttpNotFound();
            
            employee.IsDeleted = true;

            var previous = db.Configuration.ValidateOnSaveEnabled;
            db.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                await db.SaveChangesAsync();
            }
            finally
            {
                db.Configuration.ValidateOnSaveEnabled = previous;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: Employees/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var employee = await db.Employees.FindAsync(id);
            if (employee == null) 
                return HttpNotFound();
            
            employee.IsDeleted = false;

            var previous = db.Configuration.ValidateOnSaveEnabled;
            db.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                await db.SaveChangesAsync();
            }
            finally
            {
                db.Configuration.ValidateOnSaveEnabled = previous;
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(RecycleBin));
        }

        // GET: Employees/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           
            var employee = await db.Employees
                .Include(e => e.Territories)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
            if (employee == null) 
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.EmployeeID == id && !od.Order.IsDeleted);

            var vm = new EmployeesViewModel
            {
                Employee = employee,
                Territories = (employee.Territories ?? new List<Territory>())
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Orders = (employee.Orders ?? new List<Order>())
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (employee.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .SelectMany(o => o.Order_Details ?? new List<Order_Detail>())
                    .Where(od => od.Product != null)
                    .Select(od => od.Product)
                    .Distinct()
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                OrderDetails = await orderDetails
                    .Where(od => !od.IsDeleted)
                    .OrderBy(od => od.OrderID)
                    .ThenBy(od => od.ProductID)
                    .ToListAsync(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };
            vm.EmployeeServedProducts = vm.Calculate();

            return View(vm);
        }

        // POST: Employees/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var employee = await db.Employees
                .Include(e => e.Territories)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
            if (employee == null) 
                return HttpNotFound();
            
            var hasOrders = employee.Orders != null && employee.Orders.Any();

            if (hasOrders) 
            {
                foreach (var order in employee.Orders) 
                    order.EmployeeID = null;
            }

            employee.Territories.Clear();
            db.Employees.Remove(employee);
            await db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(RecycleBin));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            
            base.Dispose(disposing);
        }
    }
}
