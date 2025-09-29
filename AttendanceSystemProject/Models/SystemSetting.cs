using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceSystemProject.Models
{
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; }

        [StringLength(500)]
        public string SettingValue { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}