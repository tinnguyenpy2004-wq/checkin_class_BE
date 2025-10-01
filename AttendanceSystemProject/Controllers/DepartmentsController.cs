using System.Linq;
using System.Net;
using System.Web.Mvc;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    [Authorize] // bắt buộc đăng nhập mới vào
    public class DepartmentsController : Controller
    {
        private readonly AttendanceSystemContext _db = new AttendanceSystemContext();

        // ============ READ ============
        // Cho Admin + Teacher (tùy bạn có muốn Student xem hay không)
        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult Index()
        {
            var list = _db.Departments
                          .OrderByDescending(d => d.IsActive)
                          .ThenBy(d => d.Name)
                          .ToList();
            return View(list);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult Details(int id)
        {
            var d = _db.Departments.Find(id);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        // ============ CREATE ============
        [Authorize(Roles = "Admin")]
        public ActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult Create(Department model)
        {
            // Kiểm tra trùng Code
            if (_db.Departments.Any(x => x.Code == model.Code))
                ModelState.AddModelError("Code", "Mã khoa/phòng đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            _db.Departments.Add(model);
            _db.SaveChanges();
            TempData["msg"] = "Tạo khoa/phòng thành công.";
            return RedirectToAction("Index");
        }

        // ============ EDIT ============
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var d = _db.Departments.Find(id);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult Edit(Department model)
        {
            // Unique Code ngoại trừ chính nó
            if (_db.Departments.Any(x => x.Code == model.Code && x.DepartmentId != model.DepartmentId))
                ModelState.AddModelError("Code", "Mã khoa/phòng đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var d = _db.Departments.Find(model.DepartmentId);
            if (d == null) return HttpNotFound();

            d.Name = model.Name;
            d.Code = model.Code;
            d.Description = model.Description;
            d.IsActive = model.IsActive;

            _db.SaveChanges();
            TempData["msg"] = "Cập nhật khoa/phòng thành công.";
            return RedirectToAction("Index");
        }

        // ============ DELETE ============
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var d = _db.Departments.Find(id);
            if (d == null) return HttpNotFound();
            return View(d);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var d = _db.Departments.Find(id);
            if (d == null) return HttpNotFound();

            _db.Departments.Remove(d);
            _db.SaveChanges();
            TempData["msg"] = "Xóa khoa/phòng thành công.";
            return RedirectToAction("Index");
        }

        // ============ Toggle kích hoạt nhanh (tùy chọn) ============
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public ActionResult ToggleActive(int id)
        {
            var d = _db.Departments.Find(id);
            if (d == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            d.IsActive = !d.IsActive;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
