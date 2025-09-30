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


namespace AttendanceSystemProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly AttendanceSystemContext _db = new AttendanceSystemContext();
        private IAuthenticationManager Auth => HttpContext.GetOwinContext().Authentication;

        [HttpGet]
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Utilities.RateLimiter.RateLimit(KeyPrefix = "register", MaxHits = 20, WindowSeconds = 600)]
        public async Task<ActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var ip = Request?.UserHostAddress ?? "unknown";
            if (!Utilities.RateLimiter.Allow($"register:{ip}", maxHits: 10, window: TimeSpan.FromMinutes(10)))
            {
                ModelState.AddModelError("", "Thao tác quá nhanh. Vui lòng thử lại sau.");
                return View(vm);
            }

            if (_db.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("", "Email đã tồn tại.");
                return View(vm);
            }
            if (_db.Users.Any(u => u.Username == vm.Email))
            {
                ModelState.AddModelError("", "Username đã tồn tại.");
                return View(vm);
            }

            using (var tx = _db.Database.BeginTransaction())
            {
                try
                {
                    var user = new User
                    {
                        Username = vm.Email,
                        FullName = $"{vm.FirstName} {vm.LastName}".Trim(),
                        Email = vm.Email,
                        PasswordHash = PasswordHasher.Hash(vm.Password),
                        Role = (int)UserRole.Student,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        EmailConfirmed = false
                    };

                    _db.Users.Add(user);
                    _db.SaveChanges();

                    if (!IsPasswordStrong(vm.Password))
                    {
                        ModelState.AddModelError("", "Mật khẩu phải tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số.");
                        tx.Rollback();
                        return View(vm);
                    }

                    var code = OtpGenerator.SixDigits();
                    var oldOtps = _db.LoginOtps.Where(o => o.UserId == user.UserId && o.Purpose == "register_confirm" && o.ConsumedAt == null && o.ExpiresAt > DateTime.Now);
                    foreach (var o in oldOtps) o.ExpiresAt = DateTime.Now;

                    _db.LoginOtps.Add(new LoginOtp
                    {
                        UserId = user.UserId,
                        Purpose = "register_confirm",
                        Code = Utilities.OtpHasher.Hash(code),
                        ExpiresAt = DateTime.Now.AddMinutes(10),
                        CreatedAt = DateTime.Now
                    });
                    _db.SaveChanges();

                    tx.Commit();

                    try
                    {
                        var html = $"<p>Xin chào <b>{user.FullName}</b>,</p>" +
                                   $"<p>Mã xác nhận của bạn là: <b>{code}</b> (hết hạn sau 10 phút).</p>";
                        await EmailSender.SendAsync(user.Email, "[Attendance] Xác thực email", html);
                    }
                    catch (Exception ex)
                    {
                        TempData["msg"] = "Đăng ký thành công, nhưng gửi email thất bại: " + ex.Message;
                    }

                    try { LogAudit("Register", user.UserId); } catch { }

                    return RedirectToAction("VerifyOtp", new { userId = user.UserId, purpose = "register_confirm" });
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    try { Utilities.FileLogger.Error("Register failed", ex); } catch { }
                    var root = (ex.GetBaseException()?.Message ?? ex.Message);
                    ModelState.AddModelError("", "Không thể đăng ký: " + root);
                    return View(vm);
                }
            }
        }

        private bool IsPasswordStrong(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            return hasLower && hasUpper && hasDigit;
        }

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Utilities.RateLimiter.RateLimit(KeyPrefix = "login", MaxHits = 40, WindowSeconds = 600)]
        public async Task<ActionResult> Login(LoginVm vm, string returnUrl)
        {
            if (!ModelState.IsValid) return View(vm);

            var ip = Request?.UserHostAddress ?? "unknown";
            if (!Utilities.RateLimiter.Allow($"login-ip:{ip}", maxHits: 20, window: TimeSpan.FromMinutes(10)) ||
                !Utilities.RateLimiter.Allow($"login-user:{vm.UsernameOrEmail}", maxHits: 10, window: TimeSpan.FromMinutes(10)))
            {
                ModelState.AddModelError("", "Thao tác quá nhanh. Vui lòng thử lại sau.");
                return View(vm);
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == vm.UsernameOrEmail);
            try
            {
                var masked = (vm.UsernameOrEmail ?? "").Trim();
                if (masked.Length > 3) masked = masked.Substring(0, 3) + "***";
                Utilities.FileLogger.Info($"Login attempt for {masked} from {Request?.UserHostAddress}");
            }
            catch { }
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại hoặc bị khóa.");
                return View(vm);
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.Now)
            {
                var remaining = user.LockoutEnd.Value - DateTime.Now;
                var minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
                ModelState.AddModelError("", $"Tài khoản đang bị khóa tạm thời. Vui lòng thử lại sau khoảng {minutes} phút.");
                return View(vm);
            }

            if (!PasswordHasher.Verify(vm.Password, user.PasswordHash))
            {
                user.AccessFailedCount = (user.AccessFailedCount + 1);
                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnd = DateTime.Now.AddMinutes(15);
                    user.AccessFailedCount = 0; // reset after locking
                }
                _db.SaveChanges();
                Utilities.FileLogger.Info($"Password failed for userId={user.UserId} ip={Request?.UserHostAddress}");
                try { LogAudit("LoginPasswordFailed", user.UserId); } catch { }
                ModelState.AddModelError("", "Mật khẩu không chính xác.");
                return View(vm);
            }

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            _db.SaveChanges();

            if (!user.EmailConfirmed)
            {
                var confirmCode = OtpGenerator.SixDigits();
                var oldConfirmOtps = _db.LoginOtps.Where(o => o.UserId == user.UserId && o.Purpose == "register_confirm" && o.ConsumedAt == null && o.ExpiresAt > DateTime.Now);
                foreach (var o in oldConfirmOtps) o.ExpiresAt = DateTime.Now;

                _db.LoginOtps.Add(new LoginOtp
                {
                    UserId = user.UserId,
                    Purpose = "register_confirm",
                    Code = Utilities.OtpHasher.Hash(confirmCode),
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

            var loginCode = OtpGenerator.SixDigits();
            var oldLoginOtps = _db.LoginOtps.Where(o => o.UserId == user.UserId && o.Purpose == "login" && o.ConsumedAt == null && o.ExpiresAt > DateTime.Now);
            foreach (var o in oldLoginOtps) o.ExpiresAt = DateTime.Now;

            _db.LoginOtps.Add(new LoginOtp
            {
                UserId = user.UserId,
                Purpose = "login",
                Code = Utilities.OtpHasher.Hash(loginCode),
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

        [HttpGet]
        public ActionResult VerifyOtp(int userId, string purpose, string returnUrl)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return View(new VerifyOtpVm { UserId = userId, Purpose = purpose, ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Utilities.RateLimiter.RateLimit(KeyPrefix = "otp-verify", MaxHits = 60, WindowSeconds = 600)]
        public ActionResult VerifyOtp(VerifyOtpVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var ip = Request?.UserHostAddress ?? "unknown";
            if (!Utilities.RateLimiter.Allow($"otp-verify-ip:{ip}", maxHits: 30, window: TimeSpan.FromMinutes(10)) ||
                !Utilities.RateLimiter.Allow($"otp-verify-user:{vm.UserId}", maxHits: 15, window: TimeSpan.FromMinutes(10)))
            {
                ModelState.AddModelError("", "Thao tác quá nhanh. Vui lòng thử lại sau.");
                return View(vm);
            }

            var now = DateTime.Now;
            var hashedInput = Utilities.OtpHasher.Hash(vm.Code);
            var otp = _db.LoginOtps.FirstOrDefault(o =>
                o.UserId == vm.UserId &&
                o.Purpose == vm.Purpose &&
                o.Code == hashedInput &&
                o.ConsumedAt == null &&
                o.ExpiresAt > now);

            if (otp == null)
            {
                var failUser = _db.Users.Find(vm.UserId);
                if (failUser != null)
                {
                    failUser.AccessFailedCount = (failUser.AccessFailedCount + 1);
                    if (failUser.AccessFailedCount >= 5)
                    {
                        failUser.LockoutEnd = DateTime.Now.AddMinutes(15);
                        failUser.AccessFailedCount = 0;
                    }
                    _db.SaveChanges();
                }
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

            if (vm.Purpose == "register_confirm")
            {
                user.EmailConfirmed = true;
                _db.SaveChanges();
                TempData["msg"] = "✅ Xác thực email thành công. Hãy đăng nhập.";
                return RedirectToAction("Login");
            }

            var identity = new ClaimsIdentity("AppCookie");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.FullName ?? user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, ((UserRole)user.Role).ToString()));


            Auth.SignIn(new AuthenticationProperties { IsPersistent = vm.RememberMe }, identity);
            try { Utilities.FileLogger.Info($"Login success userId={user.UserId} ip={Request?.UserHostAddress}"); } catch { }
            try { LogAudit("LoginSuccess", user.UserId); } catch { }
            user.LastLoginDate = DateTime.Now;
            _db.SaveChanges();

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        public ActionResult Logout()
        {
            Auth.SignOut("AppCookie");
            try { Utilities.FileLogger.Info($"Logout user ip={Request?.UserHostAddress}"); } catch { }
            try
            {
                int actorId = 0;
                var claimsIdentity = User?.Identity as ClaimsIdentity;
                var idValue = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idValue, out actorId))
                {
                    LogAudit("Logout", actorId);
                }
            }
            catch { }
            return RedirectToAction("Login");
        }

        private void LogAudit(string action, int? targetUserId)
        {
            try
            {
                int actorId = 0;
                var claimsIdentity = User?.Identity as ClaimsIdentity;
                var idValue = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(idValue, out actorId);
                var ip = Request?.UserHostAddress;
                _db.AuditLogs.Add(new AuditLog
                {
                    ActorUserId = actorId,
                    TargetUserId = targetUserId,
                    Action = action,
                    IpAddress = ip,
                    CreatedAt = DateTime.Now
                });
                _db.SaveChanges();
            }
            catch { }
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Utilities.RateLimiter.RateLimit(KeyPrefix = "forgot", MaxHits = 20, WindowSeconds = 600)]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Vui lòng nhập email.");
                return View();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == email && u.IsActive);
            if (user != null)
            {
                try
                {
                    var code = OtpGenerator.SixDigits();
                    var old = _db.LoginOtps.Where(o => o.UserId == user.UserId && o.Purpose == "password_reset" && o.ConsumedAt == null && o.ExpiresAt > DateTime.Now);
                    foreach (var o in old) o.ExpiresAt = DateTime.Now;
                    _db.LoginOtps.Add(new LoginOtp
                    {
                        UserId = user.UserId,
                        Purpose = "password_reset",
                        Code = Utilities.OtpHasher.Hash(code),
                        ExpiresAt = DateTime.Now.AddMinutes(10),
                        CreatedAt = DateTime.Now
                    });
                    _db.SaveChanges();

                    var resetUrl = Url.Action("ResetPassword", "Account", new { userId = user.UserId, purpose = "password_reset" }, protocol: Request?.Url?.Scheme);
                    var html = $"<p>Mã đặt lại mật khẩu: <b>{code}</b> (10 phút).</p><p>Hoặc truy cập: <a href=\"{resetUrl}\">{resetUrl}</a></p>";
                    await EmailSender.SendAsync(user.Email, "[Attendance] Đặt lại mật khẩu", html);
                }
                catch { }
            }

            TempData["msg"] = "Nếu email hợp lệ, mã đặt lại đã được gửi.";
            return RedirectToAction("Login");
        }

        // ===== RESET PASSWORD =====
        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPassword(int userId, string purpose)
        {
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return View(new ResetPasswordVm { UserId = userId, Purpose = string.IsNullOrEmpty(purpose) ? "password_reset" : purpose });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Utilities.RateLimiter.RateLimit(KeyPrefix = "reset", MaxHits = 40, WindowSeconds = 600)]
        public async Task<ActionResult> ResetPassword(ResetPasswordVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (!IsPasswordStrong(vm.NewPassword))
            {
                ModelState.AddModelError("", "Mật khẩu phải tối thiểu 8 ký tự, gồm chữ hoa, chữ thường, số.");
                return View(vm);
            }

            var now = DateTime.Now;
            var hashedInput = Utilities.OtpHasher.Hash(vm.Code);
            var otp = _db.LoginOtps.FirstOrDefault(o =>
                o.UserId == vm.UserId &&
                o.Purpose == (vm.Purpose ?? "password_reset") &&
                o.Code == hashedInput &&
                o.ConsumedAt == null &&
                o.ExpiresAt > now);

            if (otp == null)
            {
                ModelState.AddModelError("", "OTP không hợp lệ hoặc đã hết hạn.");
                return View(vm);
            }

            var user = _db.Users.Find(vm.UserId);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản không hợp lệ.");
                return View(vm);
            }

            otp.ConsumedAt = now;
            user.PasswordHash = PasswordHasher.Hash(vm.NewPassword);
            user.LastPasswordChangeAt = now;
            _db.SaveChanges();

            try
            {
                await EmailSender.SendAsync(user.Email, "[Attendance] Mật khẩu đã được thay đổi", "<p>Bạn vừa thay đổi mật khẩu. Nếu không phải bạn, vui lòng liên hệ hỗ trợ ngay.</p>");
            }
            catch { }

            try { LogAudit("PasswordReset", user.UserId); } catch { }

            TempData["msg"] = "✅ Đổi mật khẩu thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }
    }
}
