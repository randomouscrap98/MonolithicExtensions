using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    /// <summary>
    /// An interface for a service to facilitate remote procedure calls. Communication is handled by the caller:
    /// this only resolves calls against a service. 
    /// </summary>
    public interface IRemoteCallService
    {
        /// <summary>
        /// Find the appropriate endpoint in <paramref name="service"/> to send <paramref name="request"/>
        /// to, then return the serialized response.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        string ResolveCall<T>(string request, T service);

        /// <summary>
        /// Create the request string that should be passed for the given call.
        /// </summary>
        /// <param name="serializedParameters"></param>
        /// <returns></returns>
        string CreateCall(MethodBase info, IEnumerable<object> parameters);

        /// <summary>
        /// Serialize object into string using whatever method the implementation gives
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        string SerializeObject<T>(T objectToSerialize);

        /// <summary>
        /// Deserialize string into object using the same method as SerializeObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToDeserialize"></param>
        /// <returns></returns>
        T DeserializeObject<T>(string objectToDeserialize);
    }

    /// <summary>
    /// A service for facilitating remote procedure calls between processes/networks/etc. (depending on the implementation)
    /// </summary>
    public interface IRemoteCallClient
    {
        /// <summary>
        /// The endpoint of the RPC server to make calls against
        /// </summary>
        string Endpoint { get; set; }
        
        /// <summary>
        /// Call the given void function against the RPC server given in Endpoint
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        void CallVoid(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null);

        /// <summary>
        /// Call the given function against the RPC server given in Endpoint
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        T Call<T>(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null);

        /// <summary>
        /// Call the given void function against the RPC server given in Endpoint without blocking
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task CallVoidAsync(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null);

        /// <summary>
        /// Call the given function against the RPC server given in Endpoint without blocking 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<T> CallAsync<T>(MethodBase method, IEnumerable<object> parameters, CancellationToken? token = null);
    }

    /// <summary>
    /// An interface for a remote procedure call server which directs calls on an address to a given set of service objects.
    /// </summary>
    public interface IRemoteCallServer
    {
        /// <summary>
        /// Begin the remote procedure call server to listen on <paramref name="baseAddress"/>. Clients can call public functions
        /// on the given service objects by combining the baseAddress with the service endpoint dictionary key and using 
        /// an IRemoteCallClient to create the serialized call data.
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="services"></param>
        void Start(string baseAddress, Dictionary<string, object> services);

        void Stop();
    }

    public class RemoteCallException : Exception
    {
        public RemoteCallException(string Message) : base(Message) { }
        public RemoteCallException(string Message, Exception InnerException) : base(Message, InnerException) { }
    }

    public class RemoteCallCommunicationException : RemoteCallException 
    {
        public RemoteCallCommunicationException(string Message) : base(Message) { }
        public RemoteCallCommunicationException(string Message, Exception InnerException) : base(Message, InnerException) { }
    }

    public class RemoteCallInternalServerException : RemoteCallException
    {
        public RemoteCallInternalServerException(string Message) : base(Message) { }
        public RemoteCallInternalServerException(string Message, Exception InnerException) : base(Message, InnerException) { }
    }

    public class GeneralRemoteCallConfig
    {
        public bool LowLevelLogging = false;
    }
}
