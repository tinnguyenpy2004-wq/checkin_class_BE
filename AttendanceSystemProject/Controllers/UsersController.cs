using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Utilities;
using System.Security.Claims;
using System.Text;

namespace AttendanceSystemProject.Controllers
{
    public class UsersController : Controller
    {
        private AttendanceSystemContext db = new AttendanceSystemContext();

        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult UserManagement(string q, int? departmentId = null, string sort = "created", string dir = "desc", int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var query = db.Users.AsNoTracking().Include("Department").AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(u =>
                    u.Username.Contains(keyword) ||
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword) ||
                    u.StudentId.Contains(keyword));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(u => u.DepartmentId == departmentId.Value);
            }

            bool desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            switch ((sort ?? "created").ToLower())
            {
                case "username":
                    query = desc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username);
                    break;
                case "fullname":
                    query = desc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName);
                    break;
                case "email":
                    query = desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                    break;
                case "department":
                    query = desc ? query.OrderByDescending(u => u.Department.Name) : query.OrderBy(u => u.Department.Name);
                    break;
                case "lastlogin":
                    query = desc ? query.OrderByDescending(u => u.LastLoginDate) : query.OrderBy(u => u.LastLoginDate);
                    break;
                case "created":
                    query = desc ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate);
                    break;
                default:
                    query = desc ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate);
                    break;
            }

            var total = query.Count();
            var skip = (page - 1) * pageSize;
            var users = query.Skip(skip).Take(pageSize).ToList();

            ViewBag.TotalItems = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Query = q;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Sort = sort;
            ViewBag.Dir = desc ? "desc" : "asc";
            ViewBag.Departments = new SelectList(db.Departments.OrderBy(d => d.Name).ToList(), "DepartmentId", "Name", departmentId);

            return View(users);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult GetUserDetailsJson(int id)
        {
            var user = db.Users.Include("Department").FirstOrDefault(u => u.UserId == id);
            if (user == null) return HttpNotFound();

            var dto = new AttendanceSystemProject.ViewModels.UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                StudentId = user.StudentId,
                DepartmentName = user.Department != null ? user.Department.Name : null,
                Role = ((UserRole)user.Role).ToString(),
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                CreatedDate = user.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                LastLoginDate = user.LastLoginDate?.ToString("yyyy-MM-ddTHH:mm:ss") ?? null
            };
            return Json(dto, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _Create()
        {
           
            ViewBag.RoleList = new SelectList(
                Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text");

            return PartialView("_UserForm", new AttendanceSystemProject.ViewModels.UserUpsertVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [AttendanceSystemProject.Utilities.RateLimiter.RateLimit(KeyPrefix = "user-create", MaxHits = 60, WindowSeconds = 600)]
        public ActionResult Create(AttendanceSystemProject.ViewModels.UserUpsertVm vm)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = vm.Username,
                    FullName = vm.FullName,
                    Email = vm.Email,
                    PhoneNumber = vm.PhoneNumber,
                    StudentId = vm.StudentId,
                    Role = vm.Role,
                    DepartmentId = vm.DepartmentId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    EmailConfirmed = false
                };

                db.Users.Add(user);
                db.SaveChanges();

                LogAudit("Create", user.UserId);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, view = RenderRazorViewToString("_UserRow", user) });
                }
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("UserManagement");
            }
            return PartialView("_UserForm", vm);
        }

        [Authorize(Roles = "Admin")]
        public PartialViewResult _Edit(int? id)
        {
            if (id == null)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return PartialView("_UserForm", new AttendanceSystemProject.ViewModels.UserUpsertVm());
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return PartialView("_UserForm", new AttendanceSystemProject.ViewModels.UserUpsertVm());
            }

      
            ViewBag.RoleList = new SelectList(
                Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int)v).ToString()
                }).ToList(), "Value", "Text", user.Role);

            var vm = new AttendanceSystemProject.ViewModels.UserUpsertVm
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                StudentId = user.StudentId,
                Role = user.Role,
                DepartmentId = user.DepartmentId
            };

            return PartialView("_UserForm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [AttendanceSystemProject.Utilities.RateLimiter.RateLimit(KeyPrefix = "user-edit", MaxHits = 120, WindowSeconds = 600)]
        public ActionResult Edit(AttendanceSystemProject.ViewModels.UserUpsertVm vm)
        {
            if (db.Users.Any(u => u.Username == vm.Username && u.UserId != vm.UserId))
            {
                ModelState.AddModelError("Username", "This username already exists.");
            }

            if (db.Users.Any(u => u.Email == vm.Email && u.UserId != vm.UserId))
            {
                ModelState.AddModelError("Email", "This email already exists.");
            }

            if (ModelState.IsValid)
            {
                var userInDb = db.Users.Find(vm.UserId);
                if (userInDb == null) return HttpNotFound();

                userInDb.Username = vm.Username;
                userInDb.FullName = vm.FullName;
                userInDb.Email = vm.Email;
                userInDb.PhoneNumber = vm.PhoneNumber;
                userInDb.StudentId = vm.StudentId;
                userInDb.Role = vm.Role;
                userInDb.DepartmentId = vm.DepartmentId;

                db.SaveChanges();

                LogAudit("Edit", userInDb.UserId);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, view = RenderRazorViewToString("_UserRow", userInDb) });
                }
                return RedirectToAction("UserManagement");
            }
            return PartialView("_UserForm", vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [AttendanceSystemProject.Utilities.RateLimiter.RateLimit(KeyPrefix = "user-delete", MaxHits = 30, WindowSeconds = 600)]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            LogAudit("Delete", id);

            if (Request.IsAjaxRequest())
            {
                return Json(new { success = true });
            }
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [AttendanceSystemProject.Utilities.RateLimiter.RateLimit(KeyPrefix = "user-lock", MaxHits = 120, WindowSeconds = 600)]
        public ActionResult Lock(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            user.IsActive = false;
            db.SaveChanges();
            LogAudit("Lock", id);
            try { EmailSender.SendAsync(user.Email, "[Attendance] Tài khoản đã bị khóa", "<p>Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên nếu đây là nhầm lẫn.</p>").Wait(); } catch { }
            if (Request.IsAjaxRequest()) return Json(new { success = true });
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [AttendanceSystemProject.Utilities.RateLimiter.RateLimit(KeyPrefix = "user-unlock", MaxHits = 120, WindowSeconds = 600)]
        public ActionResult Unlock(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            user.IsActive = true;
            db.SaveChanges();
            LogAudit("Unlock", id);
            try { EmailSender.SendAsync(user.Email, "[Attendance] Tài khoản đã được mở khóa", "<p>Tài khoản của bạn đã được mở khóa.</p>").Wait(); } catch { }
            if (Request.IsAjaxRequest()) return Json(new { success = true });
            return RedirectToAction("UserManagement");
        }

        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet]
        public ActionResult ExportCsv(string q, int? departmentId = null, string sort = "created", string dir = "desc")
        {
            var query = db.Users.AsNoTracking().Include("Department").AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(u =>
                    u.Username.Contains(keyword) ||
                    u.FullName.Contains(keyword) ||
                    u.Email.Contains(keyword) ||
                    u.StudentId.Contains(keyword));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(u => u.DepartmentId == departmentId.Value);
            }

            bool desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            switch ((sort ?? "created").ToLower())
            {
                case "username":
                    query = desc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username);
                    break;
                case "fullname":
                    query = desc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName);
                    break;
                case "email":
                    query = desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                    break;
                case "department":
                    query = desc ? query.OrderByDescending(u => u.Department.Name) : query.OrderBy(u => u.Department.Name);
                    break;
                case "lastlogin":
                    query = desc ? query.OrderByDescending(u => u.LastLoginDate) : query.OrderBy(u => u.LastLoginDate);
                    break;
                case "created":
                default:
                    query = desc ? query.OrderByDescending(u => u.CreatedDate) : query.OrderBy(u => u.CreatedDate);
                    break;
            }

            var items = query.ToList();
            var sb = new StringBuilder();
            sb.AppendLine("UserId,Username,FullName,Email,PhoneNumber,StudentId,Department,Role,IsActive,EmailConfirmed,CreatedDate,LastLoginDate");
            foreach (var u in items)
            {
                string Escape(string s)
                {
                    if (s == null) return string.Empty;
                    var needs = s.Contains(",") || s.Contains("\n") || s.Contains("\r") || s.Contains("\"");
                    var esc = s.Replace("\"", "\"\"");
                    return needs ? "\"" + esc + "\"" : esc;
                }
                sb.AppendLine(string.Join(",",
                    u.UserId.ToString(),
                    Escape(u.Username),
                    Escape(u.FullName),
                    Escape(u.Email),
                    Escape(u.PhoneNumber),
                    Escape(u.StudentId),
                    Escape(u.Department != null ? u.Department.Name : null),
                    Escape(((UserRole)u.Role).ToString()),
                    u.IsActive ? "true" : "false",
                    u.EmailConfirmed ? "true" : "false",
                    u.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    u.LastLoginDate.HasValue ? u.LastLoginDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : string.Empty
                ));
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "users.csv");
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

        private void LogAudit(string action, int targetUserId)
        {
            try
            {
                int actorId = 0;
                if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claimsIdentity = User.Identity as ClaimsIdentity;
                    var idValue = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    int.TryParse(idValue, out actorId);
                }
                var ip = Request?.UserHostAddress;
                db.AuditLogs.Add(new AuditLog
                {
                    ActorUserId = actorId,
                    TargetUserId = targetUserId,
                    Action = action,
                    IpAddress = ip,
                    CreatedAt = DateTime.Now
                });
                db.SaveChanges();
            }
            catch { }
        }
    }
}