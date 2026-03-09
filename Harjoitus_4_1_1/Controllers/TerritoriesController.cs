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
    public class TerritoriesController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Territories
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var territories = db.Territories
                .Where(t => !t.IsDeleted)
                .Include(t => t.Region)
                .OrderBy(t => t.TerritoryID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await territories.ToListAsync());
        }

        // GET: Territories/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var territories = db.Territories
                .Where(t => t.IsDeleted)
                .Include(t => t.Region)
                .OrderBy(t => t.TerritoryID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await territories.ToListAsync());
        }

        // GET: Territories/Details/5
        public async Task<ActionResult> Details(string id, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var territory = await db.Territories
                .Include(t => t.Employees)
                .Include(t => t.Region)
                .FirstOrDefaultAsync(t => t.TerritoryID == id);
            if (territory == null)
                return HttpNotFound();
            
            var vm = new TerritoriesViewModel
            {
                Territory = territory,
                Employees = (territory.Employees ?? new List<Employee>())
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.EmployeeID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };
            
            return View(vm);
        }

        // GET: Territories/Create
        public ActionResult Create(int? regionId, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", regionId);
            return View();
        }

        // POST: Territories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TerritoryID,TerritoryDescription,RegionID")] 
            Territory territory, 
            string returnUrl = null)
        {
            territory.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Territories.Add(territory);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", territory.RegionID);
            return View(territory);
        }

        // GET: Territories/Edit/5
        public async Task<ActionResult> Edit(string id = null, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var territory = await db.Territories
                .Include(t => t.Employees)
                .Include(t => t.Region)
                .FirstOrDefaultAsync(t => t.TerritoryID == id);
            if (territory == null)
                return HttpNotFound();

            var vm = new TerritoriesViewModel
            {
                Territory = territory,
                Employees = (territory.Employees ?? new List<Employee>())
                   .Where(e => !e.IsDeleted)
                   .OrderBy(e => e.EmployeeID)
                   .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", territory.RegionID);
            return View(vm);
        }

        // POST: Territories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            TerritoriesViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Territory == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Territories
                .Include(t => t.Employees)
                .Include(t => t.Region)
                .FirstOrDefaultAsync(t => t.TerritoryID == vm.Territory.TerritoryID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Territory", new[]
            { 
                "TerritoryDescription", 
                "RegionID" 
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

            vm.Territory = itemToUpdate;
            vm.Employees = (itemToUpdate.Employees ?? new List<Employee>())
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.EmployeeID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", itemToUpdate.RegionID);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Territories/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(string id, string returnUrl = null)
        {
            var territory = await db.Territories
                .FindAsync(id);
            if (territory == null) 
                return HttpNotFound();
            
            territory.IsDeleted = true;

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

            return RedirectToAction("Index");
        }

        // POST: Territories/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(string id, string returnUrl = null)
        {
            var territory = await db.Territories
                .FindAsync(id);
            if (territory == null) 
                return HttpNotFound();
            
            territory.IsDeleted = false;

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

            return RedirectToAction("Index");
        }

        // GET: Territories/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(string id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var territory = await db.Territories
                .Include(t => t.Employees)
                .Include(t => t.Region)
                .FirstOrDefaultAsync(t => t.TerritoryID == id);
            if (territory == null) 
                return HttpNotFound();

            var vm = new TerritoriesViewModel
            {
                Territory = territory,
                Employees = (territory.Employees ?? new List<Employee>())
                   .Where(e => !e.IsDeleted)
                   .OrderBy(e => e.EmployeeID)
                   .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Territories/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(string id, string returnUrl = null)
        {
            var territory = await db.Territories
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(t => t.TerritoryID == id);
            if (territory == null) 
                return HttpNotFound();
            
            var hasEmployees = territory.Employees != null && territory.Employees.Any();

            if (hasEmployees)
            {
                foreach (var employee in territory.Employees)
                    employee.Territories.Remove(territory);
            }

            db.Territories.Remove(territory);
            await db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            
            base.Dispose(disposing);
        }
    }
}
