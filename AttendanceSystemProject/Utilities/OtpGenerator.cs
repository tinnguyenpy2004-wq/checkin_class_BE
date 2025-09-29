using System;
using System.Security.Cryptography;

namespace AttendanceSystemProject.Utilities
{
    public static class OtpGenerator
    {
        public static string SixDigits()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var value = BitConverter.ToUInt32(bytes, 0) % 1000000;
                return value.ToString("D6");
            }
        }
    }
}
