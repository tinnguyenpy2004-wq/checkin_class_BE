using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Services;
using System.Data.Entity;

namespace AttendanceSystemProject.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseService dbService = new DatabaseService();

        public ActionResult Index()
        {
            // Ẩn thông báo kết nối/đếm dữ liệu để giao diện gọn nhẹ

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ClearData()
        {
            try
            {
                using (var db = new AttendanceSystemContext())
                {
                    // Xóa theo thứ tự ràng buộc khóa ngoại
                    db.Database.ExecuteSqlCommand("DELETE FROM Attendances");
                    db.Database.ExecuteSqlCommand("DELETE FROM Certificates");
                    db.Database.ExecuteSqlCommand("DELETE FROM EventFeedbacks");
                    db.Database.ExecuteSqlCommand("DELETE FROM EventParticipants");
                    db.Database.ExecuteSqlCommand("DELETE FROM ClassStudents");
                    db.Database.ExecuteSqlCommand("DELETE FROM ClassSessions");
                    db.Database.ExecuteSqlCommand("DELETE FROM Classes");
                    db.Database.ExecuteSqlCommand("DELETE FROM Events");
                    db.Database.ExecuteSqlCommand("DELETE FROM LoginOtps");
                    db.Database.ExecuteSqlCommand("DELETE FROM AuditLogs");
                    db.Database.ExecuteSqlCommand("DELETE FROM Departments");
                    db.Database.ExecuteSqlCommand("DELETE FROM SystemSettings");
                }
                TempData["Message"] = "Đã xóa toàn bộ dữ liệu mẫu (trừ Users).";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Xóa dữ liệu thất bại: " + ex.Message;
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