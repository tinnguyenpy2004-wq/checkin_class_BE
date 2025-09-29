using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Services;

namespace AttendanceSystemProject.Controllers
{
    public class DepartmentsController : Controller
    {
        private DatabaseService dbService = new DatabaseService();

        // GET: Departments
        public ActionResult Index()
        {
            try
            {
                var departments = dbService.GetAllDepartments();
                ViewBag.Message = "Kết nối database thành công!";
                return View(departments);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi kết nối database: " + ex.Message;
                return View(new List<Department>());
            }
        }

        // GET: Departments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Departments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Department department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    department.CreatedDate = DateTime.Now;
                    int newId = dbService.AddDepartment(department);
                    TempData["Success"] = "Thêm khoa thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi thêm khoa: " + ex.Message);
                }
            }

            return View(department);
        }
    }
}