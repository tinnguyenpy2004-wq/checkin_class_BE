using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Data.Entity;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
namespace AttendanceSystemProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            System.Web.Mvc.MvcHandler.DisableMvcResponseHeader = true;
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            TryEnsureSchema();
            StartOtpCleanupJob();
        }

        protected void Application_BeginRequest()
        {
            try
            {
                var cid = Guid.NewGuid().ToString("N");
                HttpContext.Current.Items["CorrelationId"] = cid;
                System.Web.HttpContext.Current.Response.Headers["X-Correlation-Id"] = cid;
                var resp = System.Web.HttpContext.Current.Response;
                // Standardize headers primarily via Web.config; keep only HSTS here.
                resp.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

                // Generate CSP nonce for this request
                var nonceBytes = Guid.NewGuid().ToByteArray();
                var nonce = Convert.ToBase64String(nonceBytes).TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
                HttpContext.Current.Items["CspNonce"] = nonce;

                // Swagger UI uses inline/eval scripts (Swashbuckle 5.x). Relax CSP only for swagger routes.
                var path = System.Web.HttpContext.Current?.Request?.Url?.AbsolutePath?.ToLowerInvariant() ?? string.Empty;
                bool isSwagger = path.Contains("/swagger") || path.Contains("/swashbuckle");
                if (isSwagger)
                {
                    // More permissive CSP for Swagger UI only
                    resp.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; font-src 'self' data:";
                }
                else
                {
                    // Strict CSP for the app pages (with nonce for any inline scripts you may add deliberately)
                    resp.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' data:; script-src 'self' 'nonce-" + nonce + "'; style-src 'self' 'unsafe-inline'";
                }
            }
            catch { }
        }

        private static System.Timers.Timer _otpCleanupTimer;

        private static void StartOtpCleanupJob()
        {
            _otpCleanupTimer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
            _otpCleanupTimer.AutoReset = true;
            _otpCleanupTimer.Elapsed += (s, e) =>
            {
                try
                {
                    using (var db = new AttendanceSystemProject.Models.AttendanceSystemContext())
                    {
                        var now = DateTime.Now;
                        var expired = db.LoginOtps.Where(o => o.ExpiresAt <= now || o.ConsumedAt != null).ToList();
                        if (expired.Count > 0)
                        {
                            db.LoginOtps.RemoveRange(expired);
                            db.SaveChanges();
                        }
                    }
                }
                catch { }
            };
            _otpCleanupTimer.Start();
        }

        private static void TryEnsureSchema()
        {
            try
            {
                using (var db = new AttendanceSystemProject.Models.AttendanceSystemContext())
                {
                    var sql = @"
IF COL_LENGTH('dbo.Users','AccessFailedCount') IS NULL
    ALTER TABLE dbo.Users ADD AccessFailedCount INT NOT NULL CONSTRAINT DF_Users_AccessFailedCount DEFAULT(0) WITH VALUES;
IF COL_LENGTH('dbo.Users','LockoutEnd') IS NULL
    ALTER TABLE dbo.Users ADD LockoutEnd DATETIME NULL;
IF COL_LENGTH('dbo.Users','LastPasswordChangeAt') IS NULL
    ALTER TABLE dbo.Users ADD LastPasswordChangeAt DATETIME NULL;
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='LoginOtps' AND COLUMN_NAME='Code' AND CHARACTER_MAXIMUM_LENGTH < 128)
    ALTER TABLE dbo.LoginOtps ALTER COLUMN Code NVARCHAR(128) NOT NULL;";
                    db.Database.ExecuteSqlCommand(sql);
                }
            }
            catch { }
        }
    }
}
