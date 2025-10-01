using System;
using System.IO;
using System.Text;
using System.Web;

namespace AttendanceSystemProject.Utilities
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();

        private static string GetLogPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var logDir = Path.Combine(baseDir, "App_Data", "logs");
            Directory.CreateDirectory(logDir);
            var file = DateTime.UtcNow.ToString("yyyy-MM-dd") + ".log";
            return Path.Combine(logDir, file);
        }

        private static string GetCorrelationId()
        {
            try { return HttpContext.Current?.Items["CorrelationId"] as string ?? "-"; } catch { return "-"; }
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message, Exception ex = null)
        {
            var full = ex == null ? message : message + "\n" + ex;
            Write("ERROR", full);
        }

        private static void Write(string level, string message)
        {
            try
            {
                var line = $"{DateTime.UtcNow:O} [{level}] [{GetCorrelationId()}] {message}";
                lock (_lock)
                {
                    File.AppendAllText(GetLogPath(), line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch { }
        }
    }
}


