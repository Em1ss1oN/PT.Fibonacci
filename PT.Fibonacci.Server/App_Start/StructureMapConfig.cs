using System.Web.Http;
using System.Web.Mvc;
using MassTransit;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PT.Fibonacci.Server.DependencyResolution;
using StructureMap;

namespace PT.Fibonacci.Server
{
    public class StructureMapConfig
    {
        public static StructureMapDependencyScope StructureMapDependencyScope { get; set; }

        public static void SetupContainer()
        {
            IContainer container = IoC.Initialize();

            StructureMapDependencyScope = new StructureMapDependencyScope(container);
            DependencyResolver.SetResolver((object) StructureMapDependencyScope);
            DynamicModuleUtility.RegisterModule(typeof(StructureMapScopeModule));
            GlobalConfiguration.Configuration.DependencyResolver =
                new StructureMapWebApiDependencyResolver(StructureMapDependencyScope.Container);
        }

        public static void DisposeContainer()
        {
            StructureMapDependencyScope.Dispose();
        }
    }
}