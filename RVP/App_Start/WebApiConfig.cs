﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RVP
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "SubjectsApi",
                routeTemplate: "api/SubjectsApi/{action}/{id}",
                defaults: new { controller= "SubjectsApi", action = "GET", id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "DashboardApi",
                routeTemplate: "api/DashboardApi/{action}/{id}",
                defaults: new { controller = "DashboardApi", action = "GET", id = RouteParameter.Optional }
            );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
