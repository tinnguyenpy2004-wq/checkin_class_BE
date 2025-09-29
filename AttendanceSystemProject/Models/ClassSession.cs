using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class ClassSession
    {
        [Key]
        public int SessionId { get; set; }

        public int ClassId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [StringLength(200)]
        public string Location { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }

        public ClassSession()
        {
            Attendances = new HashSet<Attendance>();
        }
    }
}