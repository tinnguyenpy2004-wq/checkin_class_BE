using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class Certificate
    {
        [Key]
        public int CertificateId { get; set; }

        public int UserId { get; set; }

        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        public string CertificateNumber { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string FilePath { get; set; }

        public bool IsSent { get; set; } = false;

        public DateTime? SentDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}