using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MonolithicExtensions.Portable;
using System.IO;

namespace MonolithicExtensions.General
{
    public class HttpRemoteCallServerConfig 
    {
        public TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(10);
    }

    public class HttpRemoteCallServer : IRemoteCallServer
    {
        //Make sure shutdown has a little more time than what the user gives.
        public static readonly TimeSpan ShutdownTimeExtend = TimeSpan.FromMilliseconds(10);

        protected ILogger Logger;
        protected HttpRemoteCallServerConfig config;
        protected GeneralRemoteCallConfig generalConfig;
        protected HttpListener listener = null;
        protected IRemoteCallService remoteService;
        protected Dictionary<string, object> availableServices;

        //Listener thread stuff
        private List<Task> currentTasks = new List<Task>();
        private readonly object currentTaskLock = new object();
        private CancellationTokenSource cancelSource = null;

        /// <summary>
        /// Must inject the IRemoteCallService to resolve calls and TWO configuration objects: one for the server, and one for general
        /// configuration of any RemoteCall service.
        /// </summary>
        /// <param name="remoteService"></param>
        /// <param name="config"></param>
        /// <param name="generalConfig"></param>
        public HttpRemoteCallServer(IRemoteCallService remoteService, HttpRemoteCallServerConfig config, GeneralRemoteCallConfig generalConfig)
        {
            Logger = LogServices.CreateLoggerFromDefault(GetType());
            this.config = config;
            this.generalConfig = generalConfig;
            this.remoteService = remoteService;
        }

        public void Start(string BaseAddress, Dictionary<string, object> services)
        {
            Logger.Trace("HttpRemoteCallServer Start");

            if (listener != null)
            {
                Logger.Warn("Tried to start when alread started!");
                throw new InvalidOperationException("RemoteCallServer already active!");
            }

            //Create and start the listener that will accept client requests
            listener = new HttpListener();

            foreach (var service in services)
            {
                var endpoint = $"{BaseAddress.TrimEnd("/".ToCharArray())}/{service.Key}";
                if (!endpoint.EndsWith("/")) endpoint += "/";
                Logger.Debug($"Setting up HttpListener to use endpoint {endpoint}...");
                listener.Prefixes.Add(endpoint);
            }

            listener.Start();
            availableServices = services;

            //Setup the main running loop to handle requests synchronously (on a different thread)
            cancelSource = new CancellationTokenSource();
            currentTasks.Add(Task.Run(() => MainLoop(cancelSource.Token), cancelSource.Token));
        }

        /// <summary>
        /// The main connection handling loop. Will stop when listener is (apparently) closed.
        /// </summary>
        private void MainLoop(CancellationToken token)
        {
            Logger.Debug($"Starting main connection handler loop...");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var context = listener.GetContext();

                    if(generalConfig.LowLevelLogging)
                        Logger.Trace($"$Received request from client: {context.Request.RemoteEndPoint}");

                    lock(currentTaskLock)
                    {
                        int oldCount = currentTasks.Count;
                        currentTasks.RemoveAll(x => x.IsCompleted || x.IsCanceled || x.IsFaulted);

                        if (currentTasks.Count != oldCount)
                            Logger.Trace($"Removed {oldCount - currentTasks.Count} completed tasks from list");

                        currentTasks.Add(Task.Run(() => HandleRequest(context, token), token));
                    }
                }
                catch(Exception ex)
                {
                    if((ex is ObjectDisposedException || ex is HttpListenerException) && token.IsCancellationRequested)
                    {
                        Logger.Debug($"Ending main loop; listener appears to be closed: {ex}");
                    }
                    else
                    {
                        Logger.Error($"An unhandled exception occurred: {ex}");
                    }
                }
            }
        }

        /// <summary>
        /// Parses data from request and passes it to the appropriate implementation endpoint. If the 
        /// implementation endpoint has a return value, that value is given in the response.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="token"></param>
        private void HandleRequest(HttpListenerContext context, CancellationToken token)
        {
            if(generalConfig.LowLevelLogging)
                Logger.Trace($"$Processing request from client: {context.Request.RemoteEndPoint}");

            try
            {
                var request = context.Request;
                var response = context.Response;

                try
                {
                    foreach(var service in availableServices)
                    {
                        if (request.Url.AbsolutePath.Trim("/".ToCharArray()).EndsWith(service.Key.Trim("/".ToCharArray())))
                        {
                            if(generalConfig.LowLevelLogging)
                                Logger.Trace($"Matched url {request.Url} to service {service.Key}");

                            var input = new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
                            var result = remoteService.ResolveCall(input, service.Value);

                            if (result == null)
                            {
                                if(generalConfig.LowLevelLogging)
                                    Logger.Trace("The resolved call most likely returned void. Writing an empty string...");
                                response.WriteEmptyResponse();
                            }
                            else
                            {
                                response.WriteJsonStringResponse(result);
                            }

                            return;
                        }
                    }

                    Logger.Warn($"Request for resource {request.Url} didn't match any services! This is probably a bug!");
                }
                catch (Exception ex)
                {
                    //Logger.Error($"Consuming exception during remote call resolution. HTTP code 500 will be returned. Exception: {ex}. HTTP code 500 will be returned.");
                    response.WriteEmptyResponse(500, ex.Message);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error while processing client request (consumed): {ex}");
            }
        }

        public void Stop()
        {
            Logger.Trace("HttpRemoteCallServer Stop");

            if(listener == null)
            {
                Logger.Warn("Tried to stop when server isn't running!");
                throw new InvalidOperationException("Server isn't running!");
            }

            //Pre-emptively stop the listener and set the cancel token so we don't accept any new connections ASAP.
            cancelSource.Cancel();
            listener.Close();
            listener = null;

            //Now wait for all the current server tasks... one of them is the main loop and should stop just fine,
            //but the others are probably stuck in the implementation and will HOPEFULLY stop really soon...
            lock (currentTaskLock)
            {
                Logger.Info($"Waiting for {currentTasks.Count} currently running tasks... max {config.ShutdownTimeout.ToSimplePhrase()}");
                if(!Task.WaitAll(currentTasks.ToArray(), (int)(config.ShutdownTimeout + ShutdownTimeExtend).TotalMilliseconds))
                {
                    Logger.Fatal("A task took too long to shutdown! Forcing a shutdown anyway... you may lose work! Memory may leak!");
                }
                else
                {
                    Logger.Info("All communicator tasks successfully shutdown");
                }

                currentTasks.Clear();
            }
        }
    }
}
