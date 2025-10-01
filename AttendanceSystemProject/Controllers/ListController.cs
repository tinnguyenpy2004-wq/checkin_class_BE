using AttendanceSystemProject.Models;
using System.Web.Mvc;
using AttendanceSystemProject.ViewModels;
using System.Linq;

[Authorize]
public class ListController : Controller
{
    private readonly AttendanceSystemContext dbContext;

    public ListController()
    {
        dbContext = new AttendanceSystemContext();
    }

    public ActionResult Index()
    {
        var role = User.IsInRole("Admin") ? "Admin" :
                   User.IsInRole("Teacher") ? "Teacher" :
                   "Student";

        var fullName = User.Identity.Name;

        string roleVi = role == "Admin" ? "quản trị viên" :
                        role == "Teacher" ? "giáo viên" :
                        "sinh viên";

        ViewBag.Greeting = $"Xin chào {roleVi} {fullName}";
        ViewBag.RoleText = $"Bạn đang đăng nhập với quyền {roleVi}.";
        ViewBag.Role = role; // Truyền role để sử dụng trong view

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
                        Role = u.Role.ToString() 
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
                return View("List", adminModel);

            case "Teacher":
                var teacherModel = new TeacherViewModel
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
                return View("List", teacherModel);

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
                return View("List", studentModel);
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