using System.Configuration;
using System.Text;
using System.Web.Mvc;

namespace AttendanceSystemProject.Controllers
{
    public class InfoController : Controller
    {
        [AllowAnonymous]
        public ActionResult Robots()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Disallow:");
            return Content(sb.ToString(), "text/plain");
        }

        [AllowAnonymous]
        public ActionResult Security()
        {
            var sb = new StringBuilder();
            var contact = (System.Environment.GetEnvironmentVariable("SECURITY_CONTACT_EMAIL") ?? ConfigurationManager.AppSettings["SecurityContactEmail"] ?? ConfigurationManager.AppSettings["EmailFrom"] ?? "security@example.com").Trim();
            sb.AppendLine("Contact: mailto:" + contact);
            sb.AppendLine("Expires: 2026-12-31T23:59:59Z");
            return Content(sb.ToString(), "text/plain");
        }
    }
}


