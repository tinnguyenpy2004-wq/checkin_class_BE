using System.Collections.Generic;

namespace AttendanceSystemProject.ViewModels
{
    public class TeacherViewModel
    {
        public List<ClassViewModel> ManagedClasses { get; set; }
        public List<EventViewModel> ManagedEvents { get; set; }
    }
}