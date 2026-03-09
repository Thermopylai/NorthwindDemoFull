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
    public class ShippersController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Shippers
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var shippers = db.Shippers
                .Where(s => !s.IsDeleted)
                .Include(s => s.Region)
                .OrderBy(s => s.ShipperID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await shippers.ToListAsync());
        }

        // GET: Shippers/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var shippers = db.Shippers
                .Where(s => s.IsDeleted)
                .Include(s => s.Region)
                .OrderBy(s => s.ShipperID);
            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await shippers.ToListAsync());
        }

        // GET: Shippers/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var shipper = await db.Shippers
                .Include(s => s.Region.Territories)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShipperID == id);
            if (shipper == null)
                return HttpNotFound();

            var vm = new ShippersViewModel
            {
                Shipper = shipper,
                Orders = (shipper.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Territories = (shipper.Region?.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            return View(vm);
        }

        // GET: Shippers/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription");
            return View();
        }

        // POST: Shippers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ShipperID,CompanyName,Phone,RegionID")] 
            Shipper shipper, 
            string returnUrl = null)
        {
            shipper.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Shippers.Add(shipper);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", shipper.RegionID);
            return View(shipper);
        }

        // GET: Shippers/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var shipper = await db.Shippers
                .Include(s => s.Region.Territories)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShipperID == id);
            if (shipper == null)
                return HttpNotFound();

            var vm = new ShippersViewModel
            {
                Shipper = shipper,
                Orders = (shipper.Orders ?? new List<Order>())
                    .Where(o => !o.IsDeleted)
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Territories = (shipper.Region?.Territories ?? new List<Territory>())
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(Index))
            };

            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", shipper.RegionID);
            return View(vm);
        }

        // POST: Shippers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            ShippersViewModel vm, 
            string returnUrl = null)
        {
            if (vm == null || vm.Shipper == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Shippers
                .Include(s => s.Region.Territories)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShipperID == vm.Shipper.ShipperID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "Shipper", new[] 
            { 
                "CompanyName", 
                "Phone", 
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

            vm.Shipper = itemToUpdate;
            vm.Orders = (itemToUpdate.Orders ?? new List<Order>())
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.OrderID)
                .ToList();
            vm.Territories = (itemToUpdate.Region?.Territories ?? new List<Territory>())
                .Where(o => !o.IsDeleted)
                .OrderBy(t => t.TerritoryID)
                .ToList();
            vm.ReturnUrl = returnUrl ?? vm.ReturnUrl ?? Url.Action(nameof(Index));

            ViewBag.ReturnUrl = vm.ReturnUrl;
            ViewBag.RegionID = new SelectList(db.Regions, "RegionID", "RegionDescription", itemToUpdate.RegionID);

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return View(vm);
        }

        // POST: Shippers/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int id, string returnUrl = null)
        {
            var shipper = await db.Shippers
                .FindAsync(id);
            if (shipper == null)
                return HttpNotFound();

            shipper.IsDeleted = true;

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

        // POST: Shippers/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int id, string returnUrl = null)
        {
            var shipper = await db.Shippers
                .FindAsync(id);
            if (shipper == null)
                return HttpNotFound();

            shipper.IsDeleted = false;

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

        // GET: Shippers/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var shipper = await db.Shippers
                .Include(s => s.Region.Territories)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShipperID == id);
            if (shipper == null)
                return HttpNotFound();

            var vm = new ShippersViewModel
            {
                Shipper = shipper,
                Orders = (shipper.Orders ?? new List<Order>())
                    .OrderBy(o => o.OrderID)
                    .ToList(),
                Territories = (shipper.Region?.Territories ?? new List<Territory>())
                    .OrderBy(t => t.TerritoryID)
                    .ToList(),
                ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
            };

            return View(vm);
        }

        // POST: Shippers/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int id, string returnUrl = null)
        {
            var shipper = await db.Shippers
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.ShipperID == id);
            if (shipper == null)
                return HttpNotFound();

            var hasShippers = shipper.Orders != null && shipper.Orders.Any();

            if (hasShippers)
            {
                ModelState.AddModelError("", "Cannot delete a shipper with existing orders. Remove or reassign the orders first!");

                var vm = new ShippersViewModel
                {
                    Shipper = shipper,
                    Orders = (shipper.Orders ?? new List<Order>())
                        .OrderBy(o => o.OrderID)
                        .ToList(),
                    Territories = (shipper.Region?.Territories ?? new List<Territory>())
                        .OrderBy(t => t.TerritoryID)
                        .ToList(),
                    ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin))
                };

                Response.StatusCode = (int)HttpStatusCode.Conflict;
                return View(nameof(DeletePermanently), vm);
            }

            db.Shippers.Remove(shipper);
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
