using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class EventFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}