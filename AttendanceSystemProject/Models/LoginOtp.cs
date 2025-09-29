using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceSystemProject.Models
{
    public class LoginOtp
    {
        [Key] // ✅ THÊM DÒNG NÀY
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OtpId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Purpose { get; set; }   // "register_confirm" | "login"

        [Required, StringLength(6)]
        public string Code { get; set; }      // 6 digits

        public DateTime ExpiresAt { get; set; }
        public DateTime? ConsumedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
