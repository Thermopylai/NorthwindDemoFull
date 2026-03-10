using Harjoitus_4_1_1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Harjoitus_4_1_1.Controllers
{
    public class OrderModalsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Orders/Create
        public ActionResult _ModalCreate()
        {
            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "CompanyName");
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "LastName");
            ViewBag.ShipVia = new SelectList(db.Shippers, "ShipperID", "CompanyName");
            return PartialView(nameof(_ModalCreate));
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _ModalCreate(
            [Bind(Include = "OrderID,CustomerID,EmployeeID,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion,ShipPostalCode,ShipCountry")]
            Order order)
        {
            order.IsDeleted = false;

            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                await db.SaveChangesAsync();

                return Json(new { ok = true });
            }

            ViewBag.CustomerID = new SelectList(db.Customers, "CustomerID", "CompanyName", order.CustomerID);
            ViewBag.EmployeeID = new SelectList(db.Employees, "EmployeeID", "LastName", order.EmployeeID);
            ViewBag.ShipVia = new SelectList(db.Shippers, "ShipperID", "CompanyName", order.ShipVia);
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return PartialView(nameof(_ModalCreate), order);
        }

        // GET: Orders/Edit/5
        public async Task<ActionResult> _ModalEdit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var order = await db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .FirstOrDefaultAsync(m => m.OrderID == id);

            if (order == null)
                return HttpNotFound();

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

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "CompanyName");
            ViewBag.EmployeeID = new SelectList(employees, "EmployeeID", "FullName");
            ViewBag.ShipVia = new SelectList(shippers, "ShipperID", "CompanyName");

            return PartialView(nameof(_ModalEdit), order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _ModalEdit(Order order)
        {
            if (order == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var itemToUpdate = await db.Orders
                .Include(o => o.Order_Details.Select(od => od.Product))
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .FirstOrDefaultAsync(m => m.OrderID == order.OrderID);

            if (itemToUpdate == null)
                return HttpNotFound();

            if (TryUpdateModel(itemToUpdate, new[]
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

                    // Re-load required navigation properties for computed totals + display names
                    // (itemToUpdate already includes Order_Details.Product currently in your code,
                    // but it doesn't include Customer/Employee/Shipper in the modal actions.)
                    await db.Entry(itemToUpdate).Reference(o => o.Customer).LoadAsync();
                    await db.Entry(itemToUpdate).Reference(o => o.Employee).LoadAsync();
                    await db.Entry(itemToUpdate).Reference(o => o.Shipper).LoadAsync();

                    var fi = new System.Globalization.CultureInfo("fi-FI");

                    // Format dates consistently (match whatever you want to show in Index)
                    string fmtDate(DateTime? d) => d.HasValue ? d.Value.ToString("d") : "";

                    return Json(new
                    {
                        ok = true,
                        orderId = itemToUpdate.OrderID,

                        // “Display” values for the Index row (what your table shows)
                        customerName = itemToUpdate.Customer != null ? itemToUpdate.Customer.CompanyName : "",
                        employeeName = itemToUpdate.Employee != null ? itemToUpdate.Employee.LastName : "",
                        shipperName = itemToUpdate.Shipper != null ? itemToUpdate.Shipper.CompanyName : "",

                        orderDate = fmtDate(itemToUpdate.OrderDate),
                        requiredDate = fmtDate(itemToUpdate.RequiredDate),
                        shippedDate = fmtDate(itemToUpdate.ShippedDate),

                        shipCountry = itemToUpdate.ShipCountry ?? "",

                        // numeric values rendered via ToString(); adjust formatting if needed
                        totalAmount = itemToUpdate.TotalAmount.ToString("C", fi),
                        totalTax = itemToUpdate.TotalTax.ToString("C", fi),
                        vatTotal = itemToUpdate.VatTotal.ToString("C", fi),
                        freight = (itemToUpdate.Freight ?? 0m).ToString("C", fi),
                        finalPrice = itemToUpdate.FinalPrice.ToString("C", fi)
                    });
                }
            }

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

            ViewBag.CustomerID = new SelectList(customers, "CustomerID", "CompanyName");
            ViewBag.EmployeeID = new SelectList(employees, "EmployeeID", "FullName");
            ViewBag.ShipVia = new SelectList(shippers, "ShipperID", "CompanyName");

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return PartialView(nameof(_ModalEdit), itemToUpdate);
        }

        // GET: Orders/DeletePermanently/5
        public async Task<ActionResult> _ModalDeletePermanently(int? id)
        {
            if (Session["UserName"] == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new
                {
                    ok = false,
                    message = "You must be logged in to delete permanently."
                }, JsonRequestBehavior.AllowGet);
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

            return PartialView(nameof(_ModalDeletePermanently), order);
        }

        // POST: Orders/DeletePermanently/5
        [HttpPost, ActionName("_ModalDeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> _ModalDeletePermanentlyConfirmed(int id)
        {
            if (Session["UserName"] == null)
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return Json(new
                {
                    ok = false,
                    message = "You must be logged in to delete permanently."
                });
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

            return Json(new { ok = true, orderId = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}