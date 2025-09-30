using System.Web;
using System.Web.Mvc;
using System;

namespace AttendanceSystemProject
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LogErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
            // Global security headers are set in Application_BeginRequest
        }
    }
}
