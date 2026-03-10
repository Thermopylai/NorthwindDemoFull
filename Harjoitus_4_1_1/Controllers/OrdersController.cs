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
    public class OrdersController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        public async Task<ActionResult> TilausOtsikot()
        {
            var orders = await db.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .OrderBy(o => o.OrderID)
                .ToListAsync();
            return View(orders);
        }

        public async Task<ActionResult> _TilausRivit(int? id)
        {
            if(id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var orderRowsList = await db.Order_Details
                .Where(od => !od.IsDeleted && od.OrderID == id.Value)
                .Include(od => od.Product)
                .Include(od => od.Product.Category)
                .OrderBy(od => od.Product.ProductName)
                .Select(od => new OrderRows 
                {
                    OrderID = id.Value,
                    UnitPrice =od.UnitPrice,
                    Quantity = od.Quantity,
                    Discount = od.Discount,
                    ProductName = od.Product.ProductName,
                    QuantityPerUnit = od.Product.QuantityPerUnit,
                    UnitsInStock = od.Product.UnitsInStock,
                    CategoryName = od.Product.Category.CategoryName,
                    Description = od.Product.Category.Description
                })
                .ToListAsync();

            return PartialView(orderRowsList);
        }

        // GET: Orders
        public async Task<ActionResult> Index(int? shipVia, string customerId = "", string shipCountry = "", string returnUrl = null)
        {
            var shippers = await db.Shippers
                .Where(s => !s.IsDeleted)
                .Select(c => new
                {
                    c.ShipperID,
                    c.CompanyName
                })
                .OrderBy(s => s.CompanyName)
                .ToListAsync();

            ViewBag.ShipVia = new SelectList(shippers, "ShipperID", "CompanyName");

            var customers = await db.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.CustomerID,
                    c.CompanyName
                })
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "CompanyName");

            var shipCountries = await db.Orders
                .Where(o => !o.IsDeleted)
                .Select(o => o.ShipCountry)
                .Where(o => o != null && o != "")
                .Distinct()
                .OrderBy(o => o)
                .ToListAsync();

            ViewBag.ShipCountry = new SelectList(shipCountries);

            var orders = db.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Where(o => customerId == "" || o.CustomerID == customerId)
                .Where(o => shipCountry == "" || o.ShipCountry == shipCountry)
                .Where(o => !shipVia.HasValue || o.ShipVia == shipVia.Value)
                .OrderBy(o => o.OrderID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await orders.ToListAsync());
        }

        // GET: Orders/RecycleBin (deleted only)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var orders = db.Orders
                .Where(o => o.IsDeleted)
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .OrderBy(o => o.OrderID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            // Entity Framework 6 does not support ThenInclude. Use Select to project the navigation properties.
            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
                return HttpNotFound();

            var vm = new OrdersViewModel
            {
                Order = order,
                OrderDetails = (order.Order_Details ?? new List<Order_Detail>())
                    .Where(od => !od.IsDeleted)
                    .OrderBy(od => od.Product.ProductName)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            
            return View(vm);
        }

        // GET: Orders/Create
        public ActionResult Create(string customerId = null, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "CompanyName", customerId);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "LastName");
            ViewBag.ShipVia = new SelectList(db.Shippers, "ShipperID", "CompanyName");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind(Include = "OrderID,CustomerID,EmployeeID,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion,ShipPostalCode,ShipCountry")] 
            Order order, 
            string returnUrl = null)
        {
            // Server-owned field
            order.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "CompanyName", order.CustomerID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "LastName", order.EmployeeID);
            ViewBag.ShipVia = new SelectList(db.Shippers, "ShipperID", "CompanyName", order.ShipVia);
            return View(order);
        }

        

        // GET: Orders/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var order = await db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefaultAsync(m => m.OrderID == id);
            if (order == null)
                return HttpNotFound();

            var vm = new OrdersViewModel
            {
                Order = order,
                OrderDetails = (order.Order_Details ?? new List<Order_Detail>())
                    .Where(od => !od.IsDeleted)
                    .OrderBy(od => od.Product.ProductName)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            
            var customers = await db.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.CustomerID,
                    c.CompanyName
                })
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var employees = await db.Employees
                .Where(e => !e.IsDeleted)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.EmployeeID + ": " + e.FirstName + " " + e.LastName
                })
                .OrderBy(e => e.EmployeeID)
                .ToListAsync();

            var shippers = await db.Shippers
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.ShipperID,
                    s.CompanyName
                })
                .OrderBy(s => s.CompanyName)
                .ToListAsync();

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "CompanyName", order.CustomerID);
            ViewBag.EmployeeID = new SelectList(employees, "EmployeeID", "FullName", order.EmployeeID);
            ViewBag.ShipVia = new SelectList(shippers, "ShipperID", "CompanyName", order.ShipVia);
            return View(vm);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            OrdersViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Order == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefaultAsync(m => m.OrderID == vm.Order.OrderID);

            if (itemToUpdate == null) 
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Order", new[]
            {
                "CustomerID",
                "EmployeeID",
                "OrderDate",
                "RequiredDate",
                "ShippedDate",
                "ShipVia",
                "Freight",
                "ShipName",
                "ShipAddress",
                "ShipCity",
                "ShipRegion",
                "ShipPostalCode",
                "ShipCountry"
            }))
            {
                if (ModelState.IsValid)
                {
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    var fallbackUrl = vm.ReturnUrl ?? Url.Action(nameof(Index));
                    return Redirect(fallbackUrl);
                }
            }
            
            vm.Order = itemToUpdate;
            vm.OrderDetails = (itemToUpdate.Order_Details ?? new List<Order_Detail>())
                .Where(od => !od.IsDeleted)
                .OrderBy(od => od.Product.ProductName)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;

            var customers = await db.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    c.CustomerID,
                    c.CompanyName
                })
                .OrderBy(c => c.CompanyName)
                .ToListAsync();

            var employees = await db.Employees
                .Where(e => !e.IsDeleted)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.EmployeeID + ": " + e.FirstName + " " + e.LastName
                })
                .OrderBy(e => e.EmployeeID)
                .ToListAsync();

            var shippers = await db.Shippers
                .Where(s => !s.IsDeleted)
                .Select(s => new
                {
                    s.ShipperID,
                    s.CompanyName
                })
                .OrderBy(s => s.CompanyName)
                .ToListAsync();

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "CompanyName", itemToUpdate.CustomerID);
            ViewBag.EmployeeID = new SelectList(employees, "EmployeeID", "FullName", itemToUpdate.EmployeeID);
            ViewBag.ShipVia = new SelectList(shippers, "ShipperID", "CompanyName", itemToUpdate.ShipVia);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        

        // POST: Orders/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var order = await db.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            if (order.Order_Details != null)
            {
                foreach (var item in order.Order_Details)
                    item.IsDeleted = true;
            }

            order.IsDeleted = true;

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

        // POST: Orders/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var order = await db.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            if (order.Order_Details != null)
            {
                foreach (var item in order.Order_Details)
                    item.IsDeleted = false;
            }
            
            order.IsDeleted = false;

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

        // GET: Orders/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            var vm = new OrdersViewModel
            {
                Order = order,
                OrderDetails = (order.Order_Details ?? new List<Order_Detail>())
                    .OrderBy(od => od.Product.ProductName)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Orders/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            var order = await db.Orders
                .Include(o => o.Order_Details)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null)
                return HttpNotFound();

            var hasOrders = order.Order_Details != null && order.Order_Details.Any();

            if (hasOrders)
                db.Order_Details.RemoveRange(order.Order_Details);

            db.Orders.Remove(order);
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
