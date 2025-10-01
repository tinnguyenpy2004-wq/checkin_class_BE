using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystemProject.ViewModels
{
    public class AdminViewModel
    {
        public List<UserViewModel> AllUsers { get; set; }
        public List<ClassViewModel> AllClasses { get; set; }
        public List<EventViewModel> AllEvents { get; set; }
        public List<DepartmentViewModel> AllDepartments { get; set; }
    }

    public class OrganizerViewModel
    {
        public List<ClassViewModel> ManagedClasses { get; set; }
        public List<EventViewModel> ManagedEvents { get; set; }
    }

    public class StudentViewModel
    {
        public List<ClassViewModel> EnrolledClasses { get; set; }
        public List<EventViewModel> ParticipatedEvents { get; set; }
    }

    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int Role { get; set; }
    }

    public class ClassViewModel
    {
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public string Code { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
    }

    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EvenName { get; set; }
        public string Code { get; set; }
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; }
    }

    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Code { get; set; }
    }
}