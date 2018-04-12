using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public class HttpRemoteCallClientConfig
    {
        public TimeSpan CommunicationTimeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Implementation of IRemoteCallClient with the most common use-case in mind.
    /// </summary>
    public class HttpRemoteCallClient : IRemoteCallClient
    {
        protected ILogger Logger;

        protected IRemoteCallService remoteService;
        protected HttpRemoteCallClientConfig config;
        protected HttpClient client;

        public string Endpoint { get; set; }

        /// <summary>
        /// Must inject the IRemoteCallService for creating calls and the configuration for communication
        /// </summary>
        /// <param name="remoteService"></param>
        /// <param name="config"></param>
        public HttpRemoteCallClient(IRemoteCallService remoteService, HttpRemoteCallClientConfig config)
        {
            Logger = LogServices.CreateLoggerFromDefault(GetType());
            this.remoteService = remoteService;
            this.config = config;
            client = new HttpClient();
            client.Timeout = config.CommunicationTimeout;
        }

        /// <summary>
        /// Attempt a POST, capture all POST exceptions and wrap them in our RemoteCallCommunicationException (so the caller doesn't have to know the underlying implementation
        /// in order to handle communication errors)
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> TryPostAsync(string endpoint, MethodBase method, IEnumerable<object> parameters, CancellationToken? token)
        {
            try
            {
                StringContent request = new StringContent(remoteService.CreateCall(method, parameters));

                if (token != null)
                    return await client.PostAsync(endpoint, request, (CancellationToken)token);
                else
                    return await client.PostAsync(endpoint, request);
            }
            catch(Exception ex)
            {
                //We assume ANY exception is a communication exception. Why else would it fail? 
                //Just in case though: log the original exception.
                Logger.Warn($"PostAsync failed in HttpRemoteCallClient. Converting to RemoteCallCommunicationException: {ex}");
                throw new RemoteCallCommunicationException("Remote call failed to reach endpoint", ex);
            }
        }

        public async Task<T> CallAsync<T>(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null)
        {
            HttpResponseMessage result = await TryPostAsync(Endpoint, method, parameters, token);
            if(result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = $"Unexpected response from server: HTTP code {result.StatusCode} : {result.ReasonPhrase}";
                Logger.Error(message);
                throw new RemoteCallInternalServerException(message);
            }
            string content = await result.Content.ReadAsStringAsync();
            return remoteService.DeserializeObject<T>(content);
        }

        public async Task CallVoidAsync(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null)
        {
            HttpResponseMessage result = await TryPostAsync(Endpoint, method, parameters, token);
            if(result.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var message = $"Unexpected response from server: HTTP code {result.StatusCode} : {result.ReasonPhrase}";
                Logger.Error(message);
                throw new RemoteCallInternalServerException(message);
            }
        }

        private Exception StripException(AggregateException ex)
        {
            Logger.Warn($"Stripping AggregateException to RemoteCallException (if possible): {ex}");
            var remoteException = ex.InnerExceptions.FirstOrDefault(x => x is RemoteCallException);

            if (remoteException != null)
                throw remoteException;
            else
                throw ex.InnerException;
        }

        public T Call<T>(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null)
        {
            try
            {
                return CallAsync<T>(method, parameters, token).Result;
            }
            catch(AggregateException ex)
            {
                throw StripException(ex);
            }
        }

        public void CallVoid(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null)
        {
            try
            {
                CallVoidAsync(method, parameters, token).Wait();
            }
            catch(AggregateException ex)
            {
                throw StripException(ex);
            }
        }
    }
}
