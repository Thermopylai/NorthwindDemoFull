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
using System.Web.WebPages;

namespace Harjoitus_4_1_1.Controllers
{
    public class Order_DetailController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Order_Detail
        public async Task<ActionResult> Index(int? productId, string customerId = "", string returnUrl = null)
        {
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

            var products = await db.Products
                .Select(p => new
                {
                    p.ProductID,
                    p.ProductName
                })
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.ProductID = new SelectList(products, "ProductID", "ProductName");

            var order_Details = db.Order_Details
                .Where(od => !od.IsDeleted)
                .Include(od => od.Order.Customer)
                .Include(od => od.Product)
                .Where(od => customerId == "" || od.Order.CustomerID == customerId)
                .Where(od => !productId.HasValue || od.ProductID == productId.Value)
                .OrderBy(od => od.OrderID)
                .ThenBy(od => od.Product.ProductName);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await order_Details.ToListAsync());
        }

        // GET: Order_Detail/RecycleBin (deleted items)
        public async Task<ActionResult> RecycleBin(string returnUrl = null)
        {
            var order_Details = db.Order_Details
                .Where(od => od.IsDeleted)
                .Include(od => od.Order.Customer)
                .Include(od => od.Product)
                .OrderBy(od => od.OrderID)
                .ThenBy(od => od.Product.ProductName);

            ViewBag.ReturnUrl = this.GetCleanReturnUrl();
            return View(await order_Details.ToListAsync());
        }

        // GET: Order_Detail/Details/5
        public async Task<ActionResult> Details(int? orderId, int? productId, string returnUrl = null)
        {
            if (orderId == null || productId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var order_Detail = await db.Order_Details.FindAsync(orderId.Value, productId.Value);
            if (order_Detail == null)
                return HttpNotFound();
            
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(order_Detail);
        }

        // GET: Order_Detail/Create
        public ActionResult Create(int? orderId, string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.OrderID = new SelectList(db.Orders, "OrderID", "OrderID", orderId);
            ViewBag.ProductID = new SelectList(db.Products, "ProductID", "ProductName");
            return View(new Order_Detail());
        }

        // POST: Order_Detail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "OrderID,ProductID,Quantity,Discount")] 
            Order_Detail order_Detail, 
            string returnUrl = null)
        {
            order_Detail.IsDeleted = false;

            if (ModelState.IsValid)
            {
                order_Detail.UnitPrice = (decimal)db.Products.Find(order_Detail.ProductID).UnitPrice;
                db.Order_Details.Add(order_Detail);
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            ViewBag.OrderID = new SelectList(db.Orders, "OrderID", "OrderID", order_Detail.OrderID);
            ViewBag.ProductID = new SelectList(db.Products, "ProductID", "ProductName", order_Detail.ProductID);
            return View(order_Detail);
        }

        // GET: Order_Detail/Edit/5
        public async Task<ActionResult> Edit(int? orderId, int? productId, string returnUrl = null)
        {
            if (orderId == null || productId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var order_Detail = await db.Order_Details.FindAsync(orderId.Value, productId.Value);
            if (order_Detail == null)
                return HttpNotFound();
            
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Index));
            return View(order_Detail);
        }

        // POST: Order_Detail/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( 
            Order_Detail order_Detail, 
            string returnUrl = null)
        {
            var itemToUpdate = await db.Order_Details.FindAsync(order_Detail.OrderID, order_Detail.ProductID);
            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, "", new[]
            {
                "Quantity",
                "Discount"
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

        // POST: Order_Detail/MoveToRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MoveToRecycleBin(int orderId, int productId, string returnUrl = null)
        {
            var order_Detail = await db.Order_Details.FindAsync(orderId, productId);
            if (order_Detail == null) 
                return HttpNotFound();
            
            order_Detail.IsDeleted = true;

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

        // POST: Order_Detail/RestoreFromRecycleBin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RestoreFromRecycleBin(int orderId, int productId, string returnUrl = null)
        {
            var order_Detail = await db.Order_Details.FindAsync(orderId, productId);
            if (order_Detail == null) 
                return HttpNotFound();
            
            order_Detail.IsDeleted = false;

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

        // GET: Order_Detail/DeletePermanently/5
        public async Task<ActionResult> DeletePermanently(int? orderId, int? productId, string returnUrl = null)
        {
            if (Session["UserName"] == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to access the delete permanently function!";
                return RedirectToAction(nameof(RecycleBin), new { returnUrl });
            }

            if (orderId == null || productId == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            
            var order_Detail = await db.Order_Details.FindAsync(orderId.Value, productId.Value);
            if (order_Detail == null)
                return HttpNotFound();
            
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(RecycleBin));
            return View(order_Detail);
        }

        // POST: Order_Detail/DeletePermanently/5
        [HttpPost, ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(int orderId, int productId, string returnUrl = null)
        {
            var order_Detail = await db.Order_Details.FindAsync(orderId, productId);
            if (order_Detail == null) 
                return HttpNotFound();
            
            db.Order_Details.Remove(order_Detail);
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
