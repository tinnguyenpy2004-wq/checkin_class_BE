using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public enum AttendanceStatus
    {
        Absent = 0,
        Present = 1,
        Late = 2,
        Excused = 3
    }

    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }

        public int UserId { get; set; }

        public int? ClassSessionId { get; set; }

        public int? EventId { get; set; }

        public AttendanceStatus Status { get; set; }

        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        [StringLength(200)]
        public string AttendanceMethod { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime RecordedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ClassSessionId")]
        public virtual ClassSession ClassSession { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}