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
using System.Security.Cryptography;
using System.Text;

namespace Harjoitus_4_1_1.Controllers
{
    public class LoginsController : Controller
    {
        private NorthwindOriginalEntities db = new NorthwindOriginalEntities();

        // GET: Logins/Login
        public async Task<ActionResult> Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View();
        }

        // POST: Logins/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(Login userLogin, string returnUrl = null) 
        {
            if (ModelState.IsValid)
            {
                var login = await db.Logins.SingleOrDefaultAsync(l => l.UserName == userLogin.UserName);
                if (login != null)
                {
                    var salt = Convert.FromBase64String(login.Salt);
                    var hashedInputPassword = HashPasswordPbkdf2(userLogin.Password, salt);
                    var storedHashedPassword = Convert.FromBase64String(login.Password);

                    if (!hashedInputPassword.SequenceEqual(storedHashedPassword))
                    {
                        ModelState.AddModelError("", "Invalid user name or password!");
                        Response.StatusCode = (int)HttpStatusCode.Conflict;
                        ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
                        return View(userLogin);
                    }
                    else
                    {
                        Session["UserName"] = login.UserName; // Store username in session
                        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);

                        return RedirectToAction(nameof(Login));
                    }
                }
            }

            ModelState.AddModelError("", "Invalid model state!");
            Response.StatusCode = (int)HttpStatusCode.Conflict;
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(userLogin);
        }

        // GET: Logins/Logout
        public async Task<ActionResult> Logout(string returnUrl = null)
        {
            var loggedUserName = Session["UserName"] as string;
            var loggedUser = db.Logins.SingleOrDefault(l => l.UserName == loggedUserName);
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(loggedUser);
        }

        // POST: Logins/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogoutConfirmed(string returnUrl = null)
        {
            Session.Abandon(); // Clear session on logout
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Login));
        }

        // GET: Logins/Details/5
        public async Task<ActionResult> Details(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Login login = await db.Logins.FindAsync(id);
            if (login == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(login);
        }

        // GET: Logins/Create
        public ActionResult Create(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View();
        }

        // POST: Logins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "LoginID,UserName,Password,Salt")] Login login, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (db.Logins.Any(u => u.UserName == login.UserName)) 
                {
                    ModelState.AddModelError("UserName", "Username already exists. Please choose a different username.");
                    ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
                    return View(login);
                }

                var salt = GenerateSalt();
                var hashedPassword = HashPasswordPbkdf2(login.Password, salt);

                login.Salt = Convert.ToBase64String(salt);
                login.Password = Convert.ToBase64String(hashedPassword);

                db.Logins.Add(login);
                await db.SaveChangesAsync();
                ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError("", "An error occurred while creating the account. Please try again.");
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(login);
        }

        private byte[] GenerateSalt(int length = 16)
        {
            var salt = new byte[length];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private byte[] HashPasswordPbkdf2(string password, byte[] salt, int iterations = 200000, int hashByteSize = 32)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return pbkdf2.GetBytes(hashByteSize);
            }
        }

        private byte[] HashPasswordSha256(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                return sha256.ComputeHash(saltedPassword);
            }
        }

        // GET: Logins/Edit/5
        public async Task<ActionResult> Edit(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Login login = await db.Logins.FindAsync(id);
            if (login == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(login);
        }

        // POST: Logins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "LoginID,UserName,Password")] Login login, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                db.Entry(login).State = EntityState.Modified;
                await db.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction(nameof(Login));
            }
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(login);
        }

        // GET: Logins/Delete/5
        public async Task<ActionResult> Delete(int? id, string returnUrl = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Login login = await db.Logins.FindAsync(id);
            if (login == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReturnUrl = returnUrl ?? Url.Action(nameof(Login));
            return View(login);
        }

        // POST: Logins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, string returnUrl = null)
        {
            Login login = await db.Logins.FindAsync(id);
            db.Logins.Remove(login);
            await db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Login));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
