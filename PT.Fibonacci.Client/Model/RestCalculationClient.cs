using System;
using System.Configuration;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace PT.Fibonacci.Client.Model
{
    public class RestCalculationClient : IRestCalculationClient
    {
        public static readonly Uri BaseUri = new Uri(ConfigurationManager.AppSettings.Get("serverUri"));

        public async Task<int> CreateNew(CancellationToken token)
        {
            var request = new RestRequest(new Uri(BaseUri, @"/new"), Method.GET);
            var response = await Execute(request, token);
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        throw new Exception(@"Failed to initialize calculation.");
                    case HttpStatusCode.OK:
                        return Int32.Parse(response.Content);
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new Exception(@"Failed to initialize calculation.");
        }

        public async Task<bool> PostNext(int id, int next, CancellationToken token)
        {
            const string failedExceptionMessage = @"Failed to calculate next.";
            var request = new RestRequest(new Uri(BaseUri, $"/{id}/{next}"), Method.POST);
            var response = await Execute(request, token);
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new Exception(@"Calculation not initialized.");
                    case HttpStatusCode.InternalServerError:
                        throw new Exception(failedExceptionMessage);
                    case HttpStatusCode.OK:
                        return true;
                    case HttpStatusCode.ResetContent:
                        return false;
                    default:
                        throw new NotSupportedException();
                }
            }

            throw new Exception(failedExceptionMessage);
        }

        public async void Complete(int id)
        {
            var request = new RestRequest(new Uri(BaseUri, $"/{id}"), Method.DELETE);
            try
            {
                await Execute(request, CancellationToken.None);
            }
            catch (Exception e)
            {
                
            }
        }

        private async Task<IRestResponse> Execute(RestRequest request, CancellationToken token)
        {
            var client = new RestClient {BaseUrl = BaseUri};
            var response = await client.ExecuteTaskAsync(request, token);
            
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            return response;
        }
    }

    public interface IRestCalculationClient
    {
        Task<int> CreateNew(CancellationToken token);
        Task<bool> PostNext(int id, int next, CancellationToken token);
        void Complete(int id);
    }

    public class RestClientException : Exception
    {
        public RestClientException()
        {
        }

        public RestClientException(string message) : base(message)
        {
        }

        public RestClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RestClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}