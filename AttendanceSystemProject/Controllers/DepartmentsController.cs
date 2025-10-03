using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AttendanceSystemProject.Models; 

namespace AttendanceSystemProject.Controllers
{

    [Authorize]
    public class DepartmentsController : Controller
    {
      
        private readonly AttendanceSystemContext _db = new AttendanceSystemContext();

   

        // GET: Departments
        // Cho phép Admin và Teacher xem danh sách khoa
        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult Index()
        {
       
            var departments = _db.Departments
                                 .OrderByDescending(d => d.IsActive) // Giả sử bạn có cột IsActive để chỉ trạng thái
                                 .ThenBy(d => d.Name)
                                 .ToList();
            return View(departments);
        }

        // GET: Departments/Details/5
        // Cho phép Admin và Teacher xem chi tiết một khoa
        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = _db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // ============ CREATE (Tạo mới) ============

        // GET: Departments/Create
        // Chỉ Admin mới có quyền tạo mới
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "DepartmentId,Name,Code,Description,IsActive")] Department department)
        {
            // Kiểm tra xem mã khoa đã tồn tại chưa
            if (_db.Departments.Any(x => x.Code == department.Code))
            {
                ModelState.AddModelError("Code", "Mã khoa này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _db.Departments.Add(department);
                _db.SaveChanges();
                TempData["msg"] = "Tạo khoa mới thành công!"; // Thông báo cho người dùng
                return RedirectToAction("Index");
            }

            return View(department);
        }

        // ============ EDIT (Chỉnh sửa) ============

        // GET: Departments/Edit/5
        // Chỉ Admin mới có quyền chỉnh sửa
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = _db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "DepartmentId,Name,Code,Description,IsActive")] Department department)
        {
            // Kiểm tra mã khoa trùng lặp (loại trừ chính nó)
            if (_db.Departments.Any(x => x.Code == department.Code && x.DepartmentId != department.DepartmentId))
            {
                ModelState.AddModelError("Code", "Mã khoa này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _db.Entry(department).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                TempData["msg"] = "Cập nhật thông tin khoa thành công!";
                return RedirectToAction("Index");
            }
            return View(department);
        }

        // ============ DELETE (Xóa) ============

        // GET: Departments/Delete/5
        // Chỉ Admin mới có quyền xóa
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department department = _db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            return View(department);
        }

        // POST: Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Department department = _db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }
            _db.Departments.Remove(department);
            _db.SaveChanges();
            TempData["msg"] = "Xóa khoa thành công!";
            return RedirectToAction("Index");
        }

        // Giải phóng tài nguyên DbContext sau khi controller hoàn thành công việc
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}