using System.Collections.Generic;

namespace AttendanceSystemProject.ViewModels
{
    public class AdminViewModel
    {
        public List<UserViewModel> AllUsers { get; set; }
        public List<ClassViewModel> AllClasses { get; set; }
        public List<EventViewModel> AllEvents { get; set; }
        public List<DepartmentViewModel> AllDepartments { get; set; }
    }

    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class DepartmentViewModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Code { get; set; }
    }
}