using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    public class HealthController : Controller
    {
        [AllowAnonymous]
        public ActionResult Healthz()
        {
            var dbOk = false;
            var smtpOk = false;
            try
            {
                using (var db = new AttendanceSystemContext())
                {
                    dbOk = db.Database.Exists();
                }
            }
            catch { dbOk = false; }

            try
            {
                var host = (Environment.GetEnvironmentVariable("SMTP_HOST") ?? ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com").Trim();
                var portStr = (Environment.GetEnvironmentVariable("SMTP_PORT") ?? ConfigurationManager.AppSettings["SmtpPort"] ?? "587").Trim();
                int port = 587; int.TryParse(portStr, out port);
                using (var client = new SmtpClient(host, port) { EnableSsl = true })
                {
                    // Just try to resolve and create client, not send
                    smtpOk = true;
                }
            }
            catch { smtpOk = false; }

            var status = (dbOk && smtpOk) ? 200 : 500;
            Response.StatusCode = status;
            return new ContentResult { Content = $"db:{dbOk}; smtp:{smtpOk}", ContentType = "text/plain" };
        }
    }
}


