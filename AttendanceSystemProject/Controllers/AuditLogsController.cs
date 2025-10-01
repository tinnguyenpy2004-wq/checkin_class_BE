using System;
using System.Linq;
using System.Web.Mvc;
using AttendanceSystemProject.Models;

namespace AttendanceSystemProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly AttendanceSystemContext db = new AttendanceSystemContext();

        public ActionResult Index(DateTime? from = null, DateTime? to = null, int? actorId = null, string action = null, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 20;

            var q = db.AuditLogs.AsNoTracking().AsQueryable();

            if (from.HasValue) q = q.Where(a => a.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(a => a.CreatedAt <= to.Value);
            if (actorId.HasValue) q = q.Where(a => a.ActorUserId == actorId.Value);
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);

            q = q.OrderByDescending(a => a.CreatedAt);

            var total = q.Count();
            var items = q.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var userIds = items.Select(i => i.ActorUserId).Concat(items.Select(i => i.TargetUserId)).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();
            var users = db.Users.Where(u => userIds.Contains(u.UserId)).Select(u => new { u.UserId, u.FullName, u.Email }).ToList();
            ViewBag.UserMap = users.ToDictionary(u => u.UserId, u => (u.FullName ?? u.Email));

            ViewBag.TotalItems = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.ActorId = actorId;
            ViewBag.ActionName = action;
            ViewBag.Actions = new[] { "Create", "Edit", "Delete", "Lock", "Unlock" };

            return View(items);
        }

        [HttpGet]
        public ActionResult ExportCsv(DateTime? from = null, DateTime? to = null, int? actorId = null, string action = null)
        {
            var q = db.AuditLogs.AsNoTracking().AsQueryable();
            if (from.HasValue) q = q.Where(a => a.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(a => a.CreatedAt <= to.Value);
            if (actorId.HasValue) q = q.Where(a => a.ActorUserId == actorId.Value);
            if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);

            var items = q.OrderByDescending(a => a.CreatedAt).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("CreatedAt,ActorUserId,TargetUserId,Action,IpAddress");
            foreach (var i in items)
            {
                var line = string.Format("{0},{1},{2},{3},{4}",
                    i.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    i.ActorUserId.HasValue ? i.ActorUserId.Value.ToString() : string.Empty,
                    i.TargetUserId.HasValue ? i.TargetUserId.Value.ToString() : string.Empty,
                    EscapeCsv(i.Action),
                    EscapeCsv(i.IpAddress));
                sb.AppendLine(line);
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "audit-logs.csv");

            string EscapeCsv(string input)
            {
                if (input == null) return string.Empty;
                var needsQuotes = input.Contains(",") || input.Contains("\n") || input.Contains("\r") || input.Contains("\"");
                var escaped = input.Replace("\"", "\"\"");
                return needsQuotes ? "\"" + escaped + "\"" : escaped;
            }
        }
    }
}


