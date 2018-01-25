using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        string CreateCall(MethodBase info, IEnumerable<object> parameters);//Dictionary<string, object> serializedParameters);

        string SerializeObject<T>(T objectToSerialize);
        T DeserializeObject<T>(string objectToDeserialize);
    }

    public interface IRemoteCallClient
    {
        Task CallVoid(MethodBase method, IEnumerable<object> parameters);
        Task<T> Call<T>(MethodBase method, IEnumerable<object> parameters);
    }

    public interface IRemoteCallServer
    {
        //object CallInterface { get; set; }
        void Start(Dictionary<string, object> services);
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

    public class RemoteCallEndpointException : RemoteCallCommunicationException
    {
        public RemoteCallEndpointException(string Message) : base(Message) { }
        public RemoteCallEndpointException(string Message, Exception InnerException) : base(Message, InnerException) { }
    }
}
