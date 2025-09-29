using System.ComponentModel.DataAnnotations;

namespace AttendanceSystemProject.ViewModels
{
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
    }

    public class VerifyOtpVm
    {
        [Required] public int UserId { get; set; }
        [Required, StringLength(6)] public string Code { get; set; }
        public string Purpose { get; set; }
        public string ReturnUrl { get; set; }
    }
}
