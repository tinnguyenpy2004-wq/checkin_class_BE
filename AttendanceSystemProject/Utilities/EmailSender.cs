using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AttendanceSystemProject.Utilities
{
    public static class EmailSender
    {
        public static async Task SendAsync(string to, string subject, string htmlBody)
        {
            var from = ConfigurationManager.AppSettings["EmailFrom"];
            var pass = ConfigurationManager.AppSettings["EmailPassword"];

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(from, pass)
            };

            using (var msg = new MailMessage(from, to)
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            })
            {
                await smtp.SendMailAsync(msg);
            }
        }
    }
}
