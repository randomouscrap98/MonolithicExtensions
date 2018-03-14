using MonolithicExtensions.Portable.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonolithicExtensions.Portable;
using System.Reflection;

namespace MonolithicExtensions.General
{
    public class JsonRemoteCallService : IRemoteCallService
    {
        protected ILogger Logger;

        public JsonRemoteCallService()
        {
            Logger = LogServices.CreateLoggerFromDefault(GetType());
        }

        private object DeserializeGeneral(string objectToDeserialize, Type t)
        {
            return MySerialize.JsonParse(objectToDeserialize, t);
        }

        public T DeserializeObject<T>(string objectToDeserialize)
        {
            return (T)DeserializeGeneral(objectToDeserialize, typeof(T));
        }

        public string SerializeObject<T>(T objectToSerialize)
        {
            return MySerialize.JsonStringify(objectToSerialize);
        }

        public string ResolveCall<T>(string request, T service)
        {
            Logger.Trace($"Resolving call for service {service.GetType()}, request: {request}");

            Type serviceType = service.GetType();
            var call = DeserializeObject<JsonRemoteCall>(request);
            MethodInfo method = serviceType.GetMethod(call.Method);
            object result = null;

            if(method == null)
            {
                Logger.Error($"No method '{call.Method}' matched in service {service.GetType()}");
                throw new InvalidOperationException("No matching method in service!");
            }

            Logger.Debug($"Matched request to method {method.Name}");

            var paramValues = new List<object>();
            var parameters = call.Parameters.ToDictionary(x => x.Key.ToLower(), y => y.Value);

            foreach (var param in method.GetParameters())
            {
                try
                {
                    paramValues.Add(DeserializeGeneral(parameters[param.Name.ToLower()], param.ParameterType));
                }
                catch(Exception ex)
                {
                    Logger.Error($"Could not deserialize parameter {param.Name}: {ex}");
                    throw new InvalidOperationException($"Could not deserialize parameter {param.Name}");
                }
            }

            result = method.Invoke(service, paramValues.ToArray());

            if (method.ReturnType == typeof(void)) //(result == null)
                return null;
            else
                return SerializeObject(result);
        }

        public string CreateCall(MethodBase info, IEnumerable<object> parameters)
        {
            int i = 0;
            var call = new JsonRemoteCall();
            call.Method = info.Name;

            foreach(var parameter in info.GetParameters())
            {
                call.Parameters.Add(parameter.Name, SerializeObject(parameters.ElementAt(i)));
                i++;
            }

            return SerializeObject(call);
        }
    }

    public class JsonRemoteCall
    {
        public string Method;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
    }
}
