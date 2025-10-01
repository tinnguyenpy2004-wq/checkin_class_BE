using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int? ActorUserId { get; set; }

        public int? TargetUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; }

        [StringLength(64)]
        public string IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}


