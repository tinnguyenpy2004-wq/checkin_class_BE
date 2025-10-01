using System.Collections.Generic;

namespace AttendanceSystemProject.ViewModels
{
    public class StudentViewModel
    {
        public List<ClassViewModel> EnrolledClasses { get; set; }
        public List<EventViewModel> ParticipatedEvents { get; set; }
    }
}