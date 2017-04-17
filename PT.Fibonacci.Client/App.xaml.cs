using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MassTransit;
using PT.Fibonacci.Client.Model;
using PT.Fibonacci.Client.ViewModel;
using StructureMap;

namespace PT.Fibonacci.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Container _container;
        private IBusControl _busControl;

        protected override void OnStartup(StartupEventArgs e)
        {
            //base.OnStartup(e);

            _container = SetupContainer();
            var viewModel = _container.GetInstance<CalculationManagerViewModel>();
            _busControl = _container.GetInstance<IBusControl>();
            
            _busControl.Start();

            var window = new MainWindow {DataContext = viewModel};
            window.Show();
        }

        private Container SetupContainer()
        {
            var container = new Container();
            container.Configure(cfg =>
            {
                cfg.ForConcreteType<CalculationManager>().Configure.Singleton();
                cfg.Forward<CalculationManager, ICalculationManager>();
                cfg.Forward<CalculationManager, ICalculationScheduler>();
                cfg.For<IRestCalculationClient>().Use<RestCalculationClient>();
                cfg.For<ICalculator>().Use<Calculator>();
                cfg.ForConcreteType<CalculationManagerViewModel>().Configure.Singleton();
                cfg.For<IMessageConsumer<ICalculation>>().Use<CalculationConsumer>();
                cfg.Forward<CalculationConsumer, IConsumer>();
                cfg.For<IBusControl>().Use(_ => CreateBusControl()).Singleton();
            });

            return container;
        }

        private IBusControl CreateBusControl()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://localhost/"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint(ConfigurationManager.AppSettings.Get("clientUri"), ec =>
                {
                    ec.LoadFrom(_container);
                });
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _busControl.Stop();
            _container.Dispose();
            base.OnExit(e);
        }
    }
}
