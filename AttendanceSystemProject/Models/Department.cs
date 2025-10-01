
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    [Table("Departments")] // map ?úng tên b?ng trong SQL
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(20)]
        public string Code { get; set; } // s? ki?m tra unique ? controller (an toàn)

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation (n?u b?n dùng)
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Class> Classes { get; set; }
    }
}
