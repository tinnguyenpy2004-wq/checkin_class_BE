using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AttendanceSystemProject
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Healthz",
                url: "healthz",
                defaults: new { controller = "Health", action = "Healthz" }
            );

            routes.MapRoute(
                name: "RobotsTxt",
                url: "robots.txt",
                defaults: new { controller = "Info", action = "Robots" }
            );

            routes.MapRoute(
                name: "SecurityTxt",
                url: ".well-known/security.txt",
                defaults: new { controller = "Info", action = "Security" }
            );

            routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Account", action = "Register", id = UrlParameter.Optional }
);

        }
    }
}
