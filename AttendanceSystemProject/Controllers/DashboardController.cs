using AttendanceSystemProject.Models;
using System.Web.Mvc;

[Authorize]
public class DashboardController : Controller
{
    public ActionResult Index()
    {
        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Organizer") ? "Organizer" :
                   "Student";

        var fullName = User.Identity.Name;

        // 👇 Mapping role sang tiếng Việt
        string roleVi = role == "Admin" ? "quản trị viên" :
                        role == "Organizer" ? "tổ chức" :
                        "sinh viên";

        ViewBag.Greeting = $"Xin chào {roleVi} {fullName}";
        ViewBag.RoleText = $"Bạn đang đăng nhập với quyền {roleVi}.";

        switch (role)
        {
            case "Admin":
                return View("AdminDashboard");
            case "Organizer":
                return View("OrganizerDashboard");
            default:
                return View("StudentDashboard");
        }
    }
}
