using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    public class UsersController : Controller
    {
        private AttendanceSystemContext db = new AttendanceSystemContext();

        public ActionResult UserManagement()
        {
            var users = db.Users.ToList();
            return View(users);
        }

        public ActionResult GetUserDetailsJson(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            var result = new
            {
                user.UserId,
                user.Username,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.StudentId,
                Role = ((UserRole)user.Role).ToString(),
                user.IsActive,
                user.EmailConfirmed,
                CreatedDate = user.CreatedDate.ToString("dd/MM/yyyy HH:mm"),
                LastLoginDate = user.LastLoginDate?.ToString("dd/MM/yyyy HH:mm") ?? "Never"
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _Create()
        {
           
            ViewBag.RoleList = new SelectList(
                Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            return PartialView("_UserForm", new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                user.CreatedDate = DateTime.Now;
                user.IsActive = true;
                user.EmailConfirmed = false;

                db.Users.Add(user);
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, view = RenderRazorViewToString("_UserRow", user) });
                }
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("UserManagement");
            }
            return PartialView("_UserForm", user);
        }

        public PartialViewResult _Edit(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_UserForm", new User());
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return PartialView("_UserForm", new User());
            }

      
            ViewBag.RoleList = new SelectList(
                Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text", user.Role);

            return PartialView("_UserForm", user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(User user)
        {
         
            if (db.Users.Any(u => u.Username == user.Username && u.UserId != user.UserId))
            {
                ModelState.AddModelError("Username", "This username already exists.");
            }

            if (db.Users.Any(u => u.Email == user.Email && u.UserId != user.UserId))
            {
                ModelState.AddModelError("Email", "This email already exists.");
            }

            if (ModelState.IsValid)
            {
                var userInDb = db.Users.Find(user.UserId);
                if (userInDb == null) return HttpNotFound();

                userInDb.Username = user.Username;
                userInDb.FullName = user.FullName;
                userInDb.Email = user.Email;
                userInDb.PhoneNumber = user.PhoneNumber;
                userInDb.StudentId = user.StudentId;
                userInDb.Role = user.Role;

                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, view = RenderRazorViewToString("_UserRow", userInDb) });
                }
                return RedirectToAction("UserManagement");
            }
            // Nếu có lỗi, trả về form với thông báo lỗi
            return PartialView("_UserForm", user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true });
            }
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Lock(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            user.IsActive = false;
            db.SaveChanges();
            if (Request.IsAjaxRequest()) return Json(new { success = true });
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unlock(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            user.IsActive = true;
            db.SaveChanges();
            if (Request.IsAjaxRequest()) return Json(new { success = true });
            return RedirectToAction("UserManagement");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}