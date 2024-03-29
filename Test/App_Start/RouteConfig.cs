﻿using System.Web.Mvc;
using System.Web.Routing;

namespace Test
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}/{group}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, group = UrlParameter.Optional },
                namespaces: new[] { "Test.Controllers" }
            );
        }
    }
}
