using System;
using System.Security.Cryptography;
using System.Text;

namespace AttendanceSystemProject.Utilities
{
    public static class OtpHasher
    {
        private static string GetSecret()
        {
            // Prefer environment variable; fallback to empty for compatibility
            return (Environment.GetEnvironmentVariable("OTP_SECRET") ?? string.Empty).Trim();
        }

        public static string Hash(string code)
        {
            if (code == null) return null;
            var secret = GetSecret();
            var input = string.Concat(code, ":", secret);
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}


