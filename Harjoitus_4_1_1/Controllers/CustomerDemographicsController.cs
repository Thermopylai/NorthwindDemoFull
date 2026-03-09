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

namespace Harjoitus_4_1_1.Controllers
{
    public class CustomerDemographicsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: CustomerDemographics
        public async Task<ActionResult> Index(string returnUrl = null)
        {
            var customerDemographics = db.CustomerDemographics
                .Where(cd => !cd.IsDeleted)
                .OrderBy(cd => cd.CustomerTypeID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await customerDemographics.ToListAsync());
        }

        // GET: CustomerDemographics/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var customerDemographics = db.CustomerDemographics
                .Where(cd => cd.IsDeleted)
                .OrderBy(cd => cd.CustomerTypeID);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await customerDemographics.ToListAsync());
        }

        // GET: CustomerDemographics/Details/5
        public async Task<ActionResult> Details(string id, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);
            if (customerDemographic == null)
                return HttpNotFound();
            
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(customerDemographic);
        }

        // GET: CustomerDemographics/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View();
        }

        // POST: CustomerDemographics/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "CustomerTypeID,CustomerDesc")] 
            CustomerDemographic customerDemographic, 
            string returnUrl = null)
        {
            customerDemographic.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.CustomerDemographics.Add(customerDemographic);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(customerDemographic);
        }

        // GET: CustomerDemographics/Edit/5
        public async Task<ActionResult> Edit(string id, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);

            if (customerDemographic == null)
                return HttpNotFound();
            
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(customerDemographic);
        }

        // POST: CustomerDemographics/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            CustomerDemographic customerDemographic, 
            string returnUrl = null)
        {
            var itemToUpdate = await db.CustomerDemographics.FindAsync(customerDemographic.CustomerTypeID);
            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "", new[]
            {
                "CustomerDesc"
            }))
            {
                if (ModelState.IsValid)
                {
                    await db.SaveChangesAsync();
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                }
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(itemToUpdate);
        }

        // POST: CustomerDemographics/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(string id, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);
            if (customerDemographic == null) 
                return HttpNotFound();
          
            customerDemographic.IsDeleted = true;

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

        // POST: CustomerDemographics/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(string id, string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);
            if (customerDemographic == null) 
                return HttpNotFound();
          
            customerDemographic.IsDeleted = false;

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

        // GET: CustomerDemographics/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(string id, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
          
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);
            if (customerDemographic == null) 
                return HttpNotFound();
          
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(customerDemographic);
        }

        // POST: CustomerDemographics/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(string id, string returnUrl = null)
        {
            var customerDemographic = await db.CustomerDemographics.FindAsync(id);
            if (customerDemographic == null) 
                return HttpNotFound();
          
            db.CustomerDemographics.Remove(customerDemographic);
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
