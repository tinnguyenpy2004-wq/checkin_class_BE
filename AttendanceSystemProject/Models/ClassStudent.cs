using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class ClassStudent
    {
        [Key]
        public int ClassStudentId { get; set; }

        public int ClassId { get; set; }

        public int StudentId { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }
    }
}