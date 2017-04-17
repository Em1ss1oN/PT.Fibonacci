using System;
using MassTransit;

namespace PT.Fibonacci.Server
{
    public static class BusControl
    {
        private static IBusControl _busControl;
        public static IBus Bus => _busControl;

        public static IBusControl Control => _busControl;

        public static void InitializeBus()
        {
            _busControl = ConfigureBus();
            StructureMapConfig.StructureMapDependencyScope.Container.Configure(cfg =>
            {
                cfg.For<IBusControl>().Use(BusControl.Control);
                cfg.Forward<IBus, IBusControl>();
            });
            _busControl.Start();
        }

        private static IBusControl ConfigureBus()
        {
            return MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            });
        }

        public static void StopBus()
        {
            _busControl.Stop(TimeSpan.FromSeconds(10));
        }
    }
}