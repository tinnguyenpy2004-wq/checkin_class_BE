using System.Collections.Generic;

namespace AttendanceSystemProject.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string EvenName { get; set; }
        public string Code { get; set; }
        public int OrganizerId { get; set; }
        public string OrganizerName { get; set; }
    }
}