using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace PT.Fibonacci.Server
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}/{current}",
                defaults: new { current = RouteParameter.Optional },
                constraints: new { current = @"\d*", id = @"\d+" }
            );
        }
    }
}
