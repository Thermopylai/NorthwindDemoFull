using Harjoitus_4_1_1.Models;
using Harjoitus_4_1_1.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Region = Harjoitus_4_1_1.Models.Region;

namespace Harjoitus_4_1_1.Controllers
{
    public class RegionsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Regions
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var regions = db.Regions
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.RegionID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await regions.ToListAsync());
        }

        // GET: Regions/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var regions = db.Regions
                .Where(r => r.IsDeleted)
                .OrderBy(r => r.RegionID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await regions.ToListAsync());
        }

        // GET: Regions/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var region = await db.Regions
                .Include(r => r.Territories)
                .Include(r => r.Shippers)
                .FirstOrDefaultAsync(r => r.RegionID == id);
            if (region == null)
                return HttpNotFound();

            var vm = new RegionsViewModel
            {
                Region = region,
                Territories = (region.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Shippers = (region.Shippers ?? new List<Shipper>())
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.ShipperID)
                    .ToList(),
                Employees = (region.Territories ?? new List<Territory>())
                    .SelectMany(t => t.Employees ?? new List<Employee>())
                    .Distinct()
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.EmployeeID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // GET: Regions/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View();
        }

        // POST: Regions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "RegionID,RegionDescription")] 
            Region region, 
            string returnUrl = null)
        {
            region.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Regions.Add(region);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(region);
        }

        // GET: Regions/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var region = await db.Regions
                .Include(r => r.Territories)
                .Include(r => r.Shippers)
                .FirstOrDefaultAsync(r => r.RegionID == id);
            if (region == null)
                return HttpNotFound();

            var vm = new RegionsViewModel
            {
                Region = region,
                Territories = (region.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Shippers = (region.Shippers ?? new List<Shipper>())
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.ShipperID)
                    .ToList(),
                Employees = (region.Territories ?? new List<Territory>())
                    .SelectMany(t => t.Employees ?? new List<Employee>())
                    .Distinct()
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.EmployeeID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // POST: Regions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            RegionsViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Region == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Regions
                .Include(r => r.Territories)
                .Include(r => r.Shippers)
                .FirstOrDefaultAsync(r => r.RegionID == vm.Region.RegionID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Region", new[] 
            { 
                "RegionDescription"
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

            vm.Region = itemToUpdate;
            vm.Territories = (itemToUpdate.Territories ?? new List<Territory>())
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.TerritoryID)
                .ToList();
            vm.Shippers = (itemToUpdate.Shippers ?? new List<Shipper>())
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.ShipperID)
                .ToList();
            vm.Employees = (itemToUpdate.Territories ?? new List<Territory>())
                .SelectMany(t => t.Employees ?? new List<Employee>())
                .Distinct()
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.EmployeeID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Regions/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var region = await db.Regions.FindAsync(id);
            if (region == null)
                return HttpNotFound();
            
            region.IsDeleted = true;

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

        // POST: Regions/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var region = await db.Regions.FindAsync(id);
            if (region == null)
                return HttpNotFound();
                        
            region.IsDeleted = false;

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

        // GET: Regions/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
           
            var region = await db.Regions
                .Include(r => r.Territories.Select(t => t.Employees))
                .Include(r => r.Shippers)
                .FirstOrDefaultAsync(r => r.RegionID == id);
            if (region == null)
                return HttpNotFound();

            var vm = new RegionsViewModel
            {
                Region = region,
                Territories = (region.Territories ?? new List<Territory>())
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                Shippers = (region.Shippers ?? new List<Shipper>())
                    .OrderBy(s => s.ShipperID)
                    .ToList(),
                Employees = (region.Territories ?? new List<Territory>())
                    .SelectMany(t => t.Employees ?? new List<Employee>())
                    .Distinct()
                    .OrderBy(e => e.EmployeeID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Regions/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var region = await db.Regions
                .Include(r => r.Territories.Select(t => t.Employees))
                .Include(r => r.Shippers)
                .FirstOrDefaultAsync(r => r.RegionID == id);

            if (region == null)
                return HttpNotFound();

            var hasTerritories = region.Territories != null && region.Territories.Any();
            var hasShippers = region.Shippers != null && region.Shippers.Any();

            if (hasTerritories || hasShippers)
            {
                ModelState.AddModelError("", "Cannot delete region. Delete or reassign all related territories, shippers and employees first!");

                var vm = new RegionsViewModel
                {
                    Region = region,
                    Territories = (region.Territories ?? new List<Territory>())
                        .OrderBy(t => t.TerritoryID)
                        .ToList(),
                    Shippers = (region.Shippers ?? new List<Shipper>())
                        .OrderBy(s => s.ShipperID)
                        .ToList(),
                    Employees = (region.Territories ?? new List<Territory>())
                        .SelectMany(t => t.Employees ?? new List<Employee>())
                        .Distinct()
                        .OrderBy(e => e.EmployeeID)
                        .ToList(),
                    ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
                };

                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return View(nameof(DeletePermanently), vm);
            }

            db.Regions.Remove(region);
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
