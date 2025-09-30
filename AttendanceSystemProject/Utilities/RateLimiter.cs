using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceSystemProject.Utilities
{
    public static class RateLimiter
    {
        private static readonly ConcurrentDictionary<string, List<DateTime>> KeyToHits = new ConcurrentDictionary<string, List<DateTime>>();

        public static bool Allow(string key, int maxHits, TimeSpan window)
        {
            var now = DateTime.UtcNow;
            var cutoff = now - window;

            var hits = KeyToHits.GetOrAdd(key, _ => new List<DateTime>());

            lock (hits)
            {
                // Remove old entries
                hits.RemoveAll(t => t < cutoff);

                if (hits.Count >= maxHits)
                {
                    return false;
                }

                hits.Add(now);
                return true;
            }
        }

        // Attribute for MVC actions
        public class RateLimitAttribute : ActionFilterAttribute
        {
            public string KeyPrefix { get; set; }
            public int MaxHits { get; set; }
            public int WindowSeconds { get; set; }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var ip = filterContext?.HttpContext?.Request?.UserHostAddress ?? "unknown";
                var actionKey = (KeyPrefix ?? "action") + ":" + ip;
                if (!Allow(actionKey, MaxHits > 0 ? MaxHits : 30, TimeSpan.FromSeconds(WindowSeconds > 0 ? WindowSeconds : 60)))
                {
                    filterContext.Result = new HttpStatusCodeResult(429, "Too Many Requests");
                }
                base.OnActionExecuting(filterContext);
            }
        }
    }
}


