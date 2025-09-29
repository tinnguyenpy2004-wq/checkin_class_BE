using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Services;

namespace AttendanceSystemProject.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseService dbService = new DatabaseService();

        public ActionResult Index()
        {
            // Test database connection
            bool isConnected = dbService.TestConnection();
            ViewBag.DatabaseConnection = isConnected ? "Kết nối database thành công!" : "Không thể kết nối database!";
            ViewBag.ConnectionStatus = isConnected ? "success" : "danger";

            // Count data
            try
            {
                ViewBag.DepartmentCount = dbService.GetDepartmentCount();
                ViewBag.UserCount = dbService.GetUserCount();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi truy vấn database: " + ex.Message;
            }

            return View();
        }

        public ActionResult SeedData()
        {
            try
            {
                dbService.SeedSampleData();
                TempData["Message"] = "Đã tạo dữ liệu mẫu thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo dữ liệu mẫu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}