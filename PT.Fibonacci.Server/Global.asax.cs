using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using MassTransit;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PT.Fibonacci.Server.DependencyResolution;
using PT.Fibonacci.Server.Models;
using StructureMap;

namespace PT.Fibonacci.Server
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            try
            {
                var initializer = new CalculationContextInitializer();
                Database.SetInitializer(initializer);
                BusControl.InitializeBus();
                //StructureMapConfig.SetupContainer();

                GlobalConfiguration.Configure(WebApiConfig.Register);
            }
            catch (System.Exception ex)
            {
                Logger.Instance.Fatal(@"Startup Error.", ex);
                throw;
            }
        }

        protected void Application_End()
        {
            BusControl.StopBus();
            //StructureMapConfig.DisposeContainer();
        }
    }
}
