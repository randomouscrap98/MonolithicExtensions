using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public class HttpRemoteCallClientConfig
    {
        public TimeSpan CommunicationTimeout = TimeSpan.FromSeconds(10);
        public string Endpoint = ""; //ALWAYS must be set!
    }

    public class HttpRemoteCallClient : IRemoteCallClient
    {
        protected ILogger Logger;

        protected IRemoteCallService remoteService;
        protected HttpRemoteCallClientConfig config;
        protected HttpClient client;

        public HttpRemoteCallClient(IRemoteCallService remoteService, HttpRemoteCallClientConfig config)
        {
            Logger = LogServices.CreateLoggerFromDefault(GetType());
            this.remoteService = remoteService;
            this.config = config;
            client = new HttpClient();
            client.Timeout = config.CommunicationTimeout;
        }

        public async Task<T> Call<T>(MethodBase method, IEnumerable<object> parameters)
        {
            string request = remoteService.CreateCall(method, parameters);
            HttpResponseMessage result = await client.PostAsync(config.Endpoint, new StringContent(request, Encoding.UTF8));
            if(result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var message = $"Unexpected response from server: HTTP code {result.StatusCode} : {result.ReasonPhrase}";
                Logger.Error(message);
                throw new RemoteCallEndpointException(message);
            }
            string content = await result.Content.ReadAsStringAsync();
            return remoteService.DeserializeObject<T>(content);
        }

        public async Task CallVoid(MethodBase method, IEnumerable<object> parameters)
        {
            string request = remoteService.CreateCall(method, parameters);
            HttpResponseMessage result = await client.PostAsync(config.Endpoint, new StringContent(request));
            if(result.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                var message = $"Unexpected response from server: HTTP code {result.StatusCode} : {result.ReasonPhrase}";
                Logger.Error(message);
                throw new RemoteCallEndpointException(message);
            }
        }
    }
}
