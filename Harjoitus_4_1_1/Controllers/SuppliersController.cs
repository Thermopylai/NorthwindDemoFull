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
    public class SuppliersController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Suppliers
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var suppliers = db.Suppliers
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await suppliers.ToListAsync());
        }

        // GET: Suppliers/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var suppliers = db.Suppliers
                .Where(s => s.IsDeleted)
                .OrderBy(s => s.SupplierID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await suppliers.ToListAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           
            var supplier = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);
            if (supplier == null)
                return HttpNotFound();

            var vm = new SuppliersViewModel
            {
                Supplier = supplier,
                Products = (supplier.Products ?? new List<Product>())
                    .Where(p => !p.Discontinued)
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            
            return View(vm);
        }

        // GET: Suppliers/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View();
        }

        // POST: Suppliers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "SupplierID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax,HomePage")] 
            Supplier supplier, 
            string returnUrl = null)
        {
            supplier.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Suppliers.Add(supplier);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);
            if (supplier == null)
                return HttpNotFound();

            var vm = new SuppliersViewModel
            {
                Supplier = supplier,
                Products = (supplier.Products ?? new List<Product>())
                    .Where(p => !p.Discontinued)
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // POST: Suppliers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            SuppliersViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Supplier == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == vm.Supplier.SupplierID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Supplier", new[]
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
                "Fax",
                "HomePage"
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

            vm.Supplier = itemToUpdate;
            vm.Products = (itemToUpdate.Products ?? new List<Product>())
                .Where(p => !p.Discontinued)
                .OrderBy(p => p.ProductID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Suppliers/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var supplier = await db.Suppliers.FindAsync(id);
            if (supplier == null)
                return HttpNotFound();

            supplier.IsDeleted = true;

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

        // POST: Suppliers/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var supplier = await db.Suppliers.FindAsync(id);
            if (supplier == null)
                return HttpNotFound();

            supplier.IsDeleted = false;

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

        // GET: Suppliers/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);
            if (supplier == null)
                return HttpNotFound();

            var vm = new SuppliersViewModel
            {
                Supplier = supplier,
                Products = (supplier.Products ?? new List<Product>())
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Suppliers/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var supplier = await db.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);
            if (supplier == null)
                return HttpNotFound();

            var hasProducts = supplier.Products != null && supplier.Products.Any();

            if (hasProducts)
            {
                ModelState.AddModelError("", "Cannot delete a supplier with existing products. Remove or reassign the products first!");

                var vm = new SuppliersViewModel
                {
                    Supplier = supplier,
                    Products = (supplier.Products ?? new List<Product>())
                        .OrderBy(p => p.ProductID)
                        .ToList(),
                    ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
                };

                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return View(nameof(DeletePermanently), vm);
            }

            db.Suppliers.Remove(supplier);
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
