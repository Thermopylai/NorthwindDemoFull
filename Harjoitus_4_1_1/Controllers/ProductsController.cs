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
    public class ProductsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Products
        public async Task<ActionResult> Index(int? categoryId, int? supplierId, string returnUrl = null)
        {
            var categories = await db.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new 
                { 
                    c.CategoryID, 
                    CategoryName = c.CategoryID + ": " + c.CategoryName
                })
                .OrderBy(c => c.CategoryID)
                .ToListAsync();

            ViewBag.CategoryID = new SelectList(categories, "CategoryID", "CategoryName");
            
            var suppliers = await db.Suppliers
                .Where(s => !s.IsDeleted)
                .Select(s => new 
                { 
                    s.SupplierID, 
                    CompanyName = s.SupplierID + ": " + s.CompanyName
                })
                .OrderBy(s => s.SupplierID)
                .ToListAsync();

            ViewBag.SupplierID = new SelectList(suppliers, "SupplierID", "CompanyName");

            var products = db.Products
                .Where(p => !p.Discontinued)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Where(p => !categoryId.HasValue || p.CategoryID == categoryId.Value)
                .Where(p => !supplierId.HasValue || p.SupplierID == supplierId.Value)
                .OrderBy(p => p.ProductID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await products.ToListAsync());
        }

        // GET: Products/RecycleBin (deleted products)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var products = db.Products
                .Where(p => p.Discontinued)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .OrderBy(p => p.ProductID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           
            var product = await db.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Order_Details.Select(o => o.Order))
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            var vm = new ProductsViewModel
            {
                Product = product,
                Orders = product.Order_Details
                    .Select(o => o.Order)
                    .Distinct()
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            
            return View(vm);
        }

        // GET: Products/Create
        public ActionResult Create(int? categoryId, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", categoryId);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel")] 
            Product product, 
            string returnUrl = null)
        {
            product.Discontinued = false;

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = await db.Products
                .Include(p => p.Order_Details.Select(o => o.Order))
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            var vm = new ProductsViewModel
            {
                Product = product,
                Orders = product.Order_Details
                    .Select(o => o.Order)
                    .Distinct()
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
            return View(vm);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            ProductsViewModel vm,
            string returnUrl = null)
        {
            if (vm == null || vm.Product == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Products
                .Include(p => p.Order_Details.Select(od => od.Order))
                .FirstOrDefaultAsync(p => p.ProductID == vm.Product.ProductID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Product", new[]
            {
                "ProductName",
                "SupplierID",
                "CategoryID",
                "QuantityPerUnit",
                "UnitPrice",
                "UnitsInStock",
                "UnitsOnOrder",
                "ReorderLevel"
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

            // Rebuild VM for redisplay (same model type as the view expects)
            vm.Product = itemToUpdate;
            vm.Orders = (itemToUpdate.Order_Details ?? new List<Order_Detail>())
                .Select(od => od.Order)
                .Distinct()
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.OrderID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", itemToUpdate.CategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", itemToUpdate.SupplierID);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Products/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            
            product.Discontinued = true;

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

        //POST: Products/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var product = await db.Products.FindAsync(id);
            if (product == null)
                return HttpNotFound();
            
            product.Discontinued = false;

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

        // GET: Products/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var product = await db.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.Order_Details.Select(o => o.Order))
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            var vm = new ProductsViewModel
            {
                Product = product,
                Orders = product.Order_Details
                    .Select(o => o.Order)
                    .Distinct()
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Products/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var product = await db.Products
                .Include(p => p.Order_Details.Select(o => o.Order))
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
                return HttpNotFound();

            var hasOrderDetails = product.Order_Details != null && product.Order_Details.Any();

            if (hasOrderDetails)
            {
                ModelState.AddModelError("", "Cannot delete a product that has associated order details. Delete or reassign order details first!");
                
                var vm = new ProductsViewModel
                {
                    Product = product,
                    Orders = product.Order_Details
                        .Select(o => o.Order)
                        .Distinct()
                        .OrderBy(o => o.OrderID)
                        .ToList(),
                    ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
                };

                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return View(nameof(DeletePermanently), vm);
            }

            db.Products.Remove(product);
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
