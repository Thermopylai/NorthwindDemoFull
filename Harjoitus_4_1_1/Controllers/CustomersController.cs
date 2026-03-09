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
    public class CustomersController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        private static string NormalizeCustomerId(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var v = id.Trim();
            return v.Length >= 5 ? v.Substring(0, 5) : v.PadRight(5);
        }

        private void PopulateCustomerDemographicsSelectList(Customer customer = null)
        {
            var allCustomerDemographics = db.CustomerDemographics
                .Select(t => new { t.CustomerTypeID, t.CustomerDesc })
                .AsEnumerable()
                .OrderBy(t => t.CustomerDesc)
                .Select(t => new
                {
                    t.CustomerTypeID,
                    CustomerDesc = (t.CustomerDesc ?? "").Trim()
                })
                .ToList();

            var selected = new List<string>();

            if (customer != null)
            {
                selected = customer.CustomerDemographics
                    .Select(t => t.CustomerTypeID)
                    .ToList();
            }

            ViewBag.CustomerTypeIDs = new MultiSelectList(
                allCustomerDemographics,
                "CustomerTypeID",
                "CustomerDesc",
                selected);
        }

        // GET: Customers
        public async Task<ActionResult> Index(string country="", string city="", string returnUrl = null)
        {
            var countries = await db.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => c.Country)
                .Where(c => c != null && c != "")
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Country = new SelectList(countries);

            var cities = await db.Customers
                .Where(c => !c.IsDeleted)
                .Select(c => c.City)
                .Where(c => c != null && c != "")
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.City = new SelectList(cities);

            var customers = db.Customers
                .Where(c => !c.IsDeleted)
                .Where(c => country == "" || c.Country == country)
                .Where(c => city == "" || c.City == city)
                .OrderBy(c => c.CustomerID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await customers.ToListAsync());
        }

        // GET: Customers/RecycleBin (deleted customers)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var customers = db.Customers
                .Where(c => c.IsDeleted)
                .OrderBy(c => c.CustomerID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await customers.ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<ActionResult> Details(string id, string returnUrl = null)
        {
            id = NormalizeCustomerId(id);
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        
            var customer = await db.Customers
                .Include(c => c.CustomerDemographics)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null)
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.CustomerID == id && !od.Order.IsDeleted);

            var vm = new CustomersViewModel 
            {
                Customer = customer,
                CustomerDemographics = (customer.CustomerDemographics ?? new List<CustomerDemographic>())
                    .Where(cd => !cd.IsDeleted)
                    .OrderBy(cd => cd.CustomerDesc)
                    .ToList(),
                Orders = (customer.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (customer.Orders ?? new List<Order>())
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
            vm.CustomerOrderedProducts = vm.Calculate();

            return View(vm);
        }

        // GET: Customers/GetCustomerForOrder?id=ALFKI
        // Returns JSON for use by AJAX (Order Create form auto-fill)
        [HttpGet]
        public async Task<ActionResult> GetCustomerForOrder(string id)
        {
            id = NormalizeCustomerId(id);
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var customer = await db.Customers
                .Where(c => c.CustomerID == id)
                .Select(c => new
                {
                    c.CustomerID,
                    c.CompanyName,
                    c.Address,
                    c.City,
                    c.Region,
                    c.PostalCode,
                    c.Country
                })
                .FirstOrDefaultAsync();
            if (customer == null)
                return HttpNotFound();

            return Json(customer, JsonRequestBehavior.AllowGet);
        }

        // GET: Customers/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            PopulateCustomerDemographicsSelectList();
            return View();
        }

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CustomerID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax")] 
            Customer customer, 
            string[] CustomerTypeID,
            string returnUrl = null)
        {
            customer.IsDeleted = false;
            if (CustomerTypeID != null)
            {
                var customerDemographics = await db.CustomerDemographics
                    .Where(cd => CustomerTypeID.Contains(cd.CustomerTypeID))
                    .ToListAsync();
                foreach (var cd in customerDemographics)
                    customer.CustomerDemographics.Add(cd);
            }
            if (ModelState.IsValid)
            {
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            PopulateCustomerDemographicsSelectList(customer);
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<ActionResult> Edit(string id, string returnUrl = null)
        {
            id = NormalizeCustomerId(id);
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var customer = await db.Customers
                .Include(c => c.CustomerDemographics)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null)
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.CustomerID == id && !od.Order.IsDeleted);

            var vm = new CustomersViewModel
            {
                Customer = customer,
                CustomerDemographics = (customer.CustomerDemographics ?? new List<CustomerDemographic>())
                    .Where(cd => !cd.IsDeleted)
                    .OrderBy(cd => cd.CustomerDesc)
                    .ToList(),
                Orders = (customer.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (customer.Orders ?? new List<Order>())
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
            vm.CustomerOrderedProducts = vm.Calculate();

            ModelState.Remove("CustomerTypeID");
            PopulateCustomerDemographicsSelectList(customer);
            return View(vm);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            CustomersViewModel vm,
            string[] CustomerTypeID,
            string returnUrl = null)
        {
            if (vm == null || vm.Customer == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Customers
                .Include(c => c.CustomerDemographics)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == vm.Customer.CustomerID);
            
            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Customer", new[] 
            { 
                "CompanyName", 
                "ContactName", 
                "ContactTitle", 
                "Address", 
                "City", 
                "Region", 
                "PostalCode", 
                "Country", 
                "Phone", 
                "Fax" 
            }))
            {
                if (Request.Form.AllKeys.Contains("CustomerTypeID"))
                {
                    var selected = CustomerTypeID ?? new string[0];
                    itemToUpdate.CustomerDemographics.Clear();
                    if (selected.Length > 0)
                    {
                        var customerDemographics = await db.CustomerDemographics
                            .Where(cd => selected.Contains(cd.CustomerTypeID))
                            .ToListAsync();
                        foreach (var cd in customerDemographics)
                            itemToUpdate.CustomerDemographics.Add(cd);
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
                .Where(od => od.Order.CustomerID == itemToUpdate.CustomerID && !od.Order.IsDeleted);

            vm.Customer = itemToUpdate;
            vm.CustomerDemographics = (itemToUpdate.CustomerDemographics ?? new List<CustomerDemographic>())
                .Where(cd => !cd.IsDeleted)
                .OrderBy(cd => cd.CustomerDesc)
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
            vm.CustomerOrderedProducts = vm.Calculate();

            ViewBag.ReturnUrl = vm.ReturnUrl;

            ModelState.Remove("CustomerTypeID");
            PopulateCustomerDemographicsSelectList(itemToUpdate);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Customers/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(string id, string returnUrl = null)
        {
            id = NormalizeCustomerId(id);
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
                return HttpNotFound();

            customer.IsDeleted = true;

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

        // POST: Customers/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(string id, string returnUrl = null)
        {
            id = NormalizeCustomerId(id);
            var customer = await db.Customers.FindAsync(id);
            if (customer == null)
                return HttpNotFound();

            customer.IsDeleted = false;

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

        // GET: Customers/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(string id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            id = NormalizeCustomerId(id);
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var customer = await db.Customers
                .Include(c => c.CustomerDemographics)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null)
                return HttpNotFound();

            var orderDetails = db.Order_Details
                .Include(od => od.Product)
                .Where(od => od.Order.CustomerID == id && !od.Order.IsDeleted);

            var vm = new CustomersViewModel
            {
                Customer = customer,
                CustomerDemographics = (customer.CustomerDemographics ?? new List<CustomerDemographic>())
                    .OrderBy(cd => cd.CustomerDesc)
                    .ToList(),
                Orders = (customer.Orders ?? new List<Order>())
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Products = (customer.Orders ?? new List<Order>())
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
            vm.CustomerOrderedProducts = vm.Calculate();

            return View(vm);
        }

        // POST: Customers/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(string id, string returnUrl = null)
        {
            id = NormalizeCustomerId(id);
            var customer = await db.Customers
                .Include(c => c.CustomerDemographics)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);
            if (customer == null)
                return HttpNotFound();

            var hasOrders = customer.Orders != null && customer.Orders.Any();

            if (hasOrders)
            {
                foreach (var order in customer.Orders)
                    order.CustomerID = null;
            }

            customer.CustomerDemographics.Clear();
            db.Customers.Remove(customer);
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
