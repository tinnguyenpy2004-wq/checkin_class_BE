using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.IO;

namespace AttendanceSystemProject.Utilities
{
    public static class EmailSender
    {
        public static async Task SendAsync(string to, string subject, string htmlBody)
        {
            var from = (System.Environment.GetEnvironmentVariable("EMAIL_FROM") ?? ConfigurationManager.AppSettings["EmailFrom"])?.Trim();
            // Allow fallback to appSetting for dev if env not set
            var pass = (System.Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? ConfigurationManager.AppSettings["EmailPassword"])?.Trim();
            var user = (System.Environment.GetEnvironmentVariable("SMTP_USER") ?? from)?.Trim();

            var host = (System.Environment.GetEnvironmentVariable("SMTP_HOST") ?? ConfigurationManager.AppSettings["SmtpHost"] ?? "").Trim();
            var portStr = (System.Environment.GetEnvironmentVariable("SMTP_PORT") ?? ConfigurationManager.AppSettings["SmtpPort"] ?? "587").Trim();
            int port = 587;
            int.TryParse(portStr, out port);

            var timeoutStr = (System.Environment.GetEnvironmentVariable("SMTP_TIMEOUT_MS") ?? ConfigurationManager.AppSettings["SmtpTimeoutMs"] ?? "15000").Trim();
            int timeoutMs = 15000; int.TryParse(timeoutStr, out timeoutMs);

            // Dev fallback: if host not configured, write to pickup directory and return
            var pickupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "emails");
            Directory.CreateDirectory(pickupDir);

            // If from not provided, try use user as from
            var fromAddress = !string.IsNullOrWhiteSpace(from) ? from : user;
            using (var msg = new MailMessage(fromAddress, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            })
            {
                // If no SMTP host configured, drop to pickup file and succeed
                if (string.IsNullOrWhiteSpace(host))
                {
                    var file = Path.Combine(pickupDir, DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff") + ".eml.txt");
                    File.WriteAllText(file, "FROM: " + fromAddress + "\nTO: " + to + "\nSUBJECT: " + subject + "\n\n" + htmlBody);
                    AttendanceSystemProject.Utilities.FileLogger.Info("Email written to pickup: " + file);
                    return;
                }
                var smtp = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Timeout = timeoutMs
                };
                if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(user, pass);
                }
                else
                {
                    smtp.UseDefaultCredentials = true;
                }

                try { await smtp.SendMailAsync(msg); }
                catch (SmtpException ex)
                {
                    try
                    {
                        AttendanceSystemProject.Utilities.FileLogger.Error("SMTP send failed: " + ex.Message, ex);
                        var file = Path.Combine(pickupDir, DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff") + ".eml.txt");
                        File.WriteAllText(file, "FROM: " + fromAddress + "\nTO: " + to + "\nSUBJECT: " + subject + "\n\n" + htmlBody);
                        AttendanceSystemProject.Utilities.FileLogger.Info("Email written to pickup after SMTP failure: " + file);
                    }
                    catch { }
                    // Swallow in dev so flow can continue
                }
            }
        }
    }
}
