namespace PT.Fibonacci.Server.DependencyResolution {
    using System.Web;

    using PT.Fibonacci.Server.App_Start;

    using StructureMap.Web.Pipeline;

    public class StructureMapScopeModule : IHttpModule {
        #region Public Methods and Operators

        public void Dispose() {
        }

        public void Init(HttpApplication context) {
            context.BeginRequest += (sender, e) => StructureMapConfig.StructureMapDependencyScope.CreateNestedContainer();
            context.EndRequest += (sender, e) => {
                HttpContextLifecycle.DisposeAndClearAll();
                StructureMapConfig.StructureMapDependencyScope.DisposeNestedContainer();
            };
        }

        #endregion
    }
}