using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Harjoitus_4_1_1.Controllers
{
    public class CategoriesController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Categories
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var categories = db.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.CategoryID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await categories.ToListAsync());
        }

        // GET: Categories/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {

            var categories = db.Categories
                .Where(c => c.IsDeleted)
                .OrderBy(c => c.CategoryID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = await db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null)
                return HttpNotFound();

            var vm = new CategoriesViewModel 
            {
                Category = category,
                Products = (category.Products ?? new List<Product>())
                    .Where(p => !p.Discontinued)
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // GET: Categories/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CategoryID,CategoryName,Description")] 
            Category category, 
            string returnUrl = null)
        {
            category.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = await db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null)
                return HttpNotFound();

            var vm = new CategoriesViewModel
            {
                Category = category,
                Products = (category.Products ?? new List<Product>())
                    .Where(p => !p.Discontinued)
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            CategoriesViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Category == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == vm.Category.CategoryID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Category", new[] 
            { 
                "CategoryName", 
                "Description" 
            }))
            {
                if (ModelState.IsValid)
                {
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    var fallbackUrl = vm.ReturnUrl ?? returnUrl ?? Url.Action(nameof(Index));
                    return Redirect(fallbackUrl);
                }
            }
            
            vm.Category = itemToUpdate;
            vm.Products = (itemToUpdate.Products ?? new List<Product>())
                .Where(p => !p.Discontinued)
                .OrderBy(p => p.ProductID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));
           
            ViewBag.ReturnUrl = vm.ReturnUrl;

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Categories/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null) 
                return HttpNotFound(); 
           
            category.IsDeleted = true;

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

        // POST: Categories/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null)
                return HttpNotFound();

            category.IsDeleted = false;

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

        // GET: Categories/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var category = await db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) 
                return HttpNotFound();

            var vm = new CategoriesViewModel
            {
                Category = category,
                Products = (category.Products ?? new List<Product>())
                    .OrderBy(p => p.ProductID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Categories/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var category = await db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
            if (category == null) 
                return HttpNotFound(); 
            
            var hasProducts = category.Products != null && category.Products.Any();

            if (hasProducts)
            {
                ModelState.AddModelError("", "Cannot delete a category that has products. Remove or reassign the products first!");

                var vm = new CategoriesViewModel
                {
                    Category = category,
                    Products = (category.Products ?? new List<Product>())
                        .OrderBy(p => p.ProductID)
                        .ToList(),
                    ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
                };

                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return View(nameof(DeletePermanently), vm);
            }               

            db.Categories.Remove(category);
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
