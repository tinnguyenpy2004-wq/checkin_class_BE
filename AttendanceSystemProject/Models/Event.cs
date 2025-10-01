using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } 

        [StringLength(50)] 
        public string Code { get; set; } 

        [StringLength(1000)]
        public string Description { get; set; }

        public int? OrganizerId { get; set; } 

        public int? DepartmentId { get; set; } 

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        public int? MaxParticipants { get; set; }

        [StringLength(50)] 
        public string Status { get; set; } 

        public bool RequiresCertificate { get; set; } 

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string QRCodeData { get; set; } 

        public DateTime? QRCodeExpiry { get; set; } 
    
        public virtual ICollection<EventParticipant> EventParticipants { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<Certificate> Certificates { get; set; }
        public virtual ICollection<EventFeedback> EventFeedbacks { get; set; }

        public Event()
        {
            EventParticipants = new HashSet<EventParticipant>();
            Attendances = new HashSet<Attendance>();
            Certificates = new HashSet<Certificate>();
            EventFeedbacks = new HashSet<EventFeedback>();
        }
    }
}