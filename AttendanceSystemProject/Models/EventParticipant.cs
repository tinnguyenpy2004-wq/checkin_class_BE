using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class EventParticipant
    {
        [Key]
        public int ParticipantId { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool IsConfirmed { get; set; } = false;

        // Navigation properties
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}