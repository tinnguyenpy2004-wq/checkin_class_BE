using System.ComponentModel.DataAnnotations;

namespace AttendanceSystemProject.ViewModels
{
    public class UserUpsertVm
    {
        public int? UserId { get; set; }
        [Required, StringLength(50)] public string Username { get; set; }
        [StringLength(100)] public string FullName { get; set; }
        [Required, EmailAddress, StringLength(100)] public string Email { get; set; }
        [StringLength(20)] public string PhoneNumber { get; set; }
        [StringLength(20)] public string StudentId { get; set; }
        [Required] public int Role { get; set; }
        public int? DepartmentId { get; set; }
    }
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string StudentId { get; set; }
        public string DepartmentName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public string CreatedDate { get; set; }
        public string LastLoginDate { get; set; }
    }
    public class RegisterVm
    {
       
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; }
    }

    public class LoginVm
    {
        [Required] public string UsernameOrEmail { get; set; }
        [Required] public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyOtpVm
    {
        [Required] public int UserId { get; set; }
        [Required, StringLength(6)] public string Code { get; set; }
        public string Purpose { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ResetPasswordVm
    {
        [Required] public int UserId { get; set; }
        [Required, StringLength(6)] public string Code { get; set; }
        public string Purpose { get; set; }
        [Required, StringLength(100, MinimumLength = 6)] public string NewPassword { get; set; }
    }
}
