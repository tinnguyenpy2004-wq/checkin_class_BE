using AttendanceSystemProject.Models;
using System.Web.Mvc;
using AttendanceSystemProject.ViewModels;
using System.Linq;

[Authorize]
public class DashboardController : Controller
{
    private readonly AttendanceSystemContext dbContext;

    public DashboardController()
    {
        dbContext = new AttendanceSystemContext(); // Khởi tạo DbContext
    }

    public ActionResult Index()
    {
        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Organizer") ? "Organizer" :
                   "Student";

        var fullName = User.Identity.Name;

        // Mapping role sang tiếng Việt
        string roleVi = role == "Admin" ? "quản trị viên" :
                        role == "Organizer" ? "tổ chức" :
                        "sinh viên";

        ViewBag.Greeting = $"Xin chào {roleVi} {fullName}";
        ViewBag.RoleText = $"Bạn đang đăng nhập với quyền {roleVi}.";

        var currentUserId = GetCurrentUserId();

        switch (role)
        {
            case "Admin":
                var adminModel = new AdminViewModel
                {
                    AllUsers = dbContext.Users.Select(u => new UserViewModel
                    {
                        UserId = u.UserId,
                        Username = u.Username,
                        FullName = u.FullName,
                        Email = u.Email,
                        Role = u.Role
                    }).ToList(),
                    AllClasses = dbContext.Classes.Select(c => new ClassViewModel
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        Code = c.ClassCode,
                        TeacherId = c.TeacherId,
                        TeacherName = dbContext.Users
                            .Where(t => t.UserId == c.TeacherId)
                            .Select(t => t.FullName)
                            .FirstOrDefault()
                    }).ToList(),
                    AllEvents = dbContext.Events.Select(e => new EventViewModel
                    {
                        EventId = e.EventId,
                        EvenName = e.Name,
                        Code = e.Code,
                        OrganizerId = (int)e.OrganizerId,
                        OrganizerName = dbContext.Users
                            .Where(o => o.UserId == e.OrganizerId)
                            .Select(o => o.FullName)
                            .FirstOrDefault()
                    }).ToList(),
                    AllDepartments = dbContext.Departments.Select(d => new DepartmentViewModel
                    {
                        DepartmentId = d.DepartmentId,
                        DepartmentName = d.Name,
                        Code = d.Code
                    }).ToList()
                };
                return View("ListForAdmin", adminModel); // Đổi tên view thành ListForAdmin

            case "Organizer":
                var organizerModel = new OrganizerViewModel
                {
                    ManagedClasses = dbContext.Classes
                        .Where(c => c.TeacherId == currentUserId)
                        .Select(c => new ClassViewModel
                        {
                            ClassId = c.ClassId,
                            ClassName = c.ClassName,
                            Code = c.ClassCode,
                            TeacherId = c.TeacherId,
                            TeacherName = fullName
                        }).ToList(),
                    ManagedEvents = dbContext.Events
                        .Where(e => e.OrganizerId == currentUserId)
                        .Select(e => new EventViewModel
                        {
                            EventId = e.EventId,
                            EvenName = e.Name,
                            Code = e.Code,
                            OrganizerId = (int)e.OrganizerId,
                            OrganizerName = fullName
                        }).ToList()
                };
                return View("ListForOrganizer", organizerModel); 

            default:
                var studentModel = new StudentViewModel
                {
                    EnrolledClasses = dbContext.ClassStudents
                        .Where(cs => cs.StudentId == currentUserId)
                        .Join(dbContext.Classes,
                            cs => cs.ClassId,
                            c => c.ClassId,
                            (cs, c) => new ClassViewModel
                            {
                                ClassId = c.ClassId,
                                ClassName = c.ClassName,
                                Code = c.ClassCode,
                                TeacherId = c.TeacherId,
                                TeacherName = dbContext.Users
                                    .Where(t => t.UserId == c.TeacherId)
                                    .Select(t => t.FullName)
                                    .FirstOrDefault()
                            })
                        .ToList(),
                    ParticipatedEvents = dbContext.EventParticipants
                        .Where(ep => ep.UserId == currentUserId)
                        .Join(dbContext.Events,
                            ep => ep.EventId,
                            e => e.EventId,
                            (ep, e) => new EventViewModel
                            {
                                EventId = e.EventId,
                                EvenName = e.Name,
                                Code = e.Code,
                                OrganizerId = (int)e.OrganizerId,
                                OrganizerName = dbContext.Users
                                    .Where(o => o.UserId == e.OrganizerId)
                                    .Select(o => o.FullName)
                                    .FirstOrDefault()
                            })
                        .ToList()
                };
                return View("ListForStudent", studentModel); // Đổi tên view thành ListForStudent
        }
    }

    private int GetCurrentUserId()
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
        return user?.UserId ?? 0;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            dbContext.Dispose();
        }
        base.Dispose(disposing);
    }
}