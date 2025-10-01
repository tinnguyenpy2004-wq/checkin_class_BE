using System;
using System.Web.Mvc;
using AttendanceSystemProject.Utilities;

namespace AttendanceSystemProject
{
    public class LogErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            try
            {
                var ex = filterContext.Exception;
                var url = filterContext.HttpContext?.Request?.Url?.ToString();
                var ip = filterContext.HttpContext?.Request?.UserHostAddress;
                FileLogger.Error("[Unhandled] " + ex.Message + "\nURL=" + url + "\nIP=" + ip + "\n" + ex);
            }
            catch { }

            base.OnException(filterContext);
        }
    }
}


