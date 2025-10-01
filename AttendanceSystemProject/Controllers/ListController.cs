using AttendanceSystemProject.Models;
using System.Web.Mvc;
using AttendanceSystemProject.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System;

[Authorize]
public class ListController : Controller
{
    private readonly AttendanceSystemContext dbContext;

    public ListController()
    {
        dbContext = new AttendanceSystemContext();
    }

    public ActionResult List()
    {
        try
        {
            var role = User.IsInRole("Admin") ? "Admin" :
                       User.IsInRole("Teacher") ? "Teacher" :
                       "Student";

            var fullName = User.Identity.Name ?? "Unknown User";

            string roleVi = role == "Admin" ? "quản trị viên" :
                            role == "Teacher" ? "giáo viên" :
                            "sinh viên";

            ViewBag.Greeting = $"Xin chào {roleVi} {fullName}";
            ViewBag.RoleText = $"Bạn đang đăng nhập với quyền {roleVi}.";
            ViewBag.Role = role;

            var currentUserId = GetCurrentUserId();

            switch (role)
            {
                case "Admin":
                    var adminModel = new AdminViewModel
                    {
                        AllUsers = dbContext.Users?.Select(u => new UserViewModel
                        {
                            UserId = u.UserId,
                            Username = u.Username,
                            FullName = u.FullName,
                            Email = u.Email,
                            Role = u.Role.ToString() 
                        }).ToList() ?? new List<UserViewModel>(),
                        AllClasses = dbContext.Classes.Include("Teacher")
                           .Select(c => new ClassViewModel
                           {
                               ClassId = c.ClassId,
                               ClassName = c.Name,
                               Code = c.Code,
                               TeacherId = c.TeacherId,
                               TeacherName = c.Teacher != null ? c.Teacher.FullName : "N/A"
                           }).ToList() ?? new List<ClassViewModel>(),
                        AllEvents = dbContext.Events?.Include("Organizer")
                           .Select(e => new EventViewModel
                           {
                               EventId = e.EventId,
                               EvenName = e.Name,
                               Code = e.Code,
                               OrganizerId = (int)e.OrganizerId,
                           }).ToList() ?? new List<EventViewModel>(),
                        AllDepartments = dbContext.Departments?.Select(d => new DepartmentViewModel
                        {
                            DepartmentId = d.DepartmentId,
                            DepartmentName = d.Name,
                            Code = d.Code
                        }).ToList() ?? new List<DepartmentViewModel>()
                    };
                    return View("List", adminModel);

                case "Teacher":
                    var teacherModel = new TeacherViewModel
                    {
                        ManagedClasses = dbContext.Classes
                            .Where(c => c.TeacherId == currentUserId)
                            .Include("Teacher")
                            .Select(c => new ClassViewModel
                            {
                                ClassId = c.ClassId,
                                ClassName = c.Name,
                                Code = c.Code,
                                TeacherId = c.TeacherId,
                                TeacherName = fullName
                            }).ToList() ?? new List<ClassViewModel>(),
                        ManagedEvents = dbContext.Events
                            .Where(e => e.OrganizerId == currentUserId)
                            .Include("Organizer")
                            .Select(e => new EventViewModel
                            {
                                EventId = e.EventId,
                                EvenName = e.Name,
                                Code = e.Code,
                                OrganizerId = (int)e.OrganizerId, 
                                OrganizerName = fullName
                            }).ToList() ?? new List<EventViewModel>()
                    };
                    return View("List", teacherModel);

                default:
                    var studentModel = new StudentViewModel
                    {
                        EnrolledClasses = dbContext.ClassStudents
                            .Where(cs => cs.StudentId == currentUserId)
                            .Join(dbContext.Classes.Include("Teacher"),
                                cs => cs.ClassId,
                                c => c.ClassId,
                                (cs, c) => new ClassViewModel
                                {
                                    ClassId = c.ClassId,
                                    ClassName = c.Name,
                                    Code = c.Code,
                                    TeacherId = c.TeacherId,
                                    TeacherName = c.Teacher != null ? c.Teacher.FullName : "N/A"
                                }).ToList() ?? new List<ClassViewModel>(),
                        ParticipatedEvents = dbContext.EventParticipants
                            .Where(ep => ep.UserId == currentUserId)
                            .Join(dbContext.Events.Include("Organizer"),
                                ep => ep.EventId,
                                e => e.EventId,
                                (ep, e) => new EventViewModel
                                {
                                    EventId = e.EventId,
                                    EvenName = e.Name,
                                    Code = e.Code,
                                    OrganizerId = (int)e.OrganizerId,
                                    
                                }).ToList() ?? new List<EventViewModel>()
                    };
                    return View("List", studentModel);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("Error in Index: " + ex.ToString());
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine("Inner Exception: " + ex.InnerException.Message);
            }
            throw;
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