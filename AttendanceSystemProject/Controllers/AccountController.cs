using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AttendanceSystemProject.Models;
using AttendanceSystemProject.Security;
using AttendanceSystemProject.Utilities;
using AttendanceSystemProject.ViewModels;
using Microsoft.Owin.Security;
using AttendanceSystemProject.Models; // để nhận enum UserRole


namespace AttendanceSystemProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly AttendanceSystemContext _db = new AttendanceSystemContext();
        private IAuthenticationManager Auth => HttpContext.GetOwinContext().Authentication;

        // ===== REGISTER =====
        [HttpGet]
        public ActionResult Register() => View();

        //[HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // 🔍 Kiểm tra trùng email
            if (_db.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("", "Email đã tồn tại.");
                return View(vm);
            }

            // ✅ Tạo user mới
            var user = new User
            {
                Username = vm.Email,
                FullName = $"{vm.FirstName} {vm.LastName}".Trim(),
                Email = vm.Email,
                PasswordHash = PasswordHasher.Hash(vm.Password),
                Role = (int)UserRole.Student, // ⚠️ dùng enum → int
                IsActive = true,
                CreatedDate = DateTime.Now,
                EmailConfirmed = false
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            // ✅ Sinh mã OTP xác thực
            var code = OtpGenerator.SixDigits();
            _db.LoginOtps.Add(new LoginOtp
            {
                UserId = user.UserId,
                Purpose = "register_confirm",
                Code = code,
                ExpiresAt = DateTime.Now.AddMinutes(10),
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            // ✉️ Gửi email xác thực
            try
            {
                var html = $"<p>Xin chào <b>{user.FullName}</b>,</p>" +
                           $"<p>Mã xác nhận của bạn là: <b>{code}</b> (hết hạn sau 10 phút).</p>";
                await EmailSender.SendAsync(user.Email, "[Attendance] Xác thực email", html);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi gửi email: " + ex.Message);
                return View(vm);
            }

            return RedirectToAction("VerifyOtp", new { userId = user.UserId, purpose = "register_confirm" });
        }

        // ===== LOGIN =====
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

       // [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginVm vm, string returnUrl)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = _db.Users.FirstOrDefault(u => u.Email == vm.UsernameOrEmail);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại hoặc bị khóa.");
                return View(vm);
            }

            // 🔐 Kiểm tra mật khẩu
            if (!PasswordHasher.Verify(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Mật khẩu không chính xác.");
                return View(vm);
            }

            // Nếu chưa xác thực email → gửi OTP xác thực
            if (!user.EmailConfirmed)
            {
                var confirmCode = OtpGenerator.SixDigits();
                _db.LoginOtps.Add(new LoginOtp
                {
                    UserId = user.UserId,
                    Purpose = "register_confirm",
                    Code = confirmCode,
                    ExpiresAt = DateTime.Now.AddMinutes(10),
                    CreatedAt = DateTime.Now
                });
                _db.SaveChanges();

                try
                {
                    var html = $"<p>Mã xác thực email của bạn là: <b>{confirmCode}</b> (hết hạn trong 10 phút).</p>";
                    await EmailSender.SendAsync(user.Email, "[Attendance] Xác thực email", html);
                }
                catch { }

                return RedirectToAction("VerifyOtp", new { userId = user.UserId, purpose = "register_confirm", returnUrl });
            }

            // Nếu đã xác thực email → gửi OTP đăng nhập
            var loginCode = OtpGenerator.SixDigits();
            _db.LoginOtps.Add(new LoginOtp
            {
                UserId = user.UserId,
                Purpose = "login",
                Code = loginCode,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                CreatedAt = DateTime.Now
            });
            _db.SaveChanges();

            try
            {
                var html = $"<p>Mã OTP đăng nhập của bạn là: <b>{loginCode}</b> (hết hạn sau 5 phút).</p>";
                await EmailSender.SendAsync(user.Email, "[Attendance] Mã OTP đăng nhập", html);
            }
            catch { }

            return RedirectToAction("VerifyOtp", new { userId = user.UserId, purpose = "login", returnUrl });
        }

        // ===== VERIFY OTP =====
        [HttpGet]
        public ActionResult VerifyOtp(int userId, string purpose, string returnUrl)
        {
            return View(new VerifyOtpVm { UserId = userId, Purpose = purpose, ReturnUrl = returnUrl });
        }

       // [HttpPost, ValidateAntiForgeryToken]
        public ActionResult VerifyOtp(VerifyOtpVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var now = DateTime.Now;
            var otp = _db.LoginOtps.FirstOrDefault(o =>
                o.UserId == vm.UserId &&
                o.Purpose == vm.Purpose &&
                o.Code == vm.Code &&
                o.ConsumedAt == null &&
                o.ExpiresAt > now);

            if (otp == null)
            {
                ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
                return View(vm);
            }

            otp.ConsumedAt = now;
            _db.SaveChanges();

            var user = _db.Users.Find(vm.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Không tìm thấy người dùng.");
                return View(vm);
            }

            // ✅ Nếu là xác thực email
            if (vm.Purpose == "register_confirm")
            {
                user.EmailConfirmed = true;
                _db.SaveChanges();
                TempData["msg"] = "✅ Xác thực email thành công. Hãy đăng nhập.";
                return RedirectToAction("Login");
            }

            // ✅ Nếu là đăng nhập
            var identity = new ClaimsIdentity("AppCookie");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName ?? user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, ((UserRole)user.Role).ToString()));


            Auth.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);
            user.LastLoginDate = DateTime.Now;
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Logout()
        {
            Auth.SignOut("AppCookie");
            return RedirectToAction("Login");
        }
    }
}
