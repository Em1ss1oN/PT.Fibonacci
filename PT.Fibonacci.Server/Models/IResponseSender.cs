using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;

namespace PT.Fibonacci.Server.Models
{
    public interface IResponseSender
    {
        Task SendResponseAsync(ICalculation calculation);
    }

    public class ResponseSender : IResponseSender
    {
        private static readonly Uri EndpointUri;

        private readonly ISendEndpointProvider _endpointProvider;

        public ResponseSender(ISendEndpointProvider endpointProvider)
        {
            _endpointProvider = endpointProvider;
        }

        static ResponseSender()
        {
            EndpointUri = new Uri(System.Web.Configuration.WebConfigurationManager.AppSettings.Get(@"clientUri"));
        }

        public async Task SendResponseAsync(ICalculation calculation)
        {
            Logger.Instance.Info($"Sending Response Id:{calculation.Id}, Current:{calculation.Current}.");
            var endpoint = await _endpointProvider.GetSendEndpoint(EndpointUri);
            await endpoint.Send(calculation);
        }
    }
}
