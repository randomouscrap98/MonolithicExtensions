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

        //public const string ParameterSeparator = "&";

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
            //return MySerialize.JsonParse<T>(objectToDeserialize);
        }

        public string SerializeObject<T>(T objectToSerialize)
        {
            return MySerialize.JsonStringify(objectToSerialize);
        }

        public string ResolveCall<T>(string request, T service)
        {
            Logger.Trace($"Resolving call for service {service.GetType()}, request: {request}");
            //IEnumerable<string> pairs = request.Split(ParameterSeparator.ToCharArray());
            //Dictionary<string, object> parameters = DeserializeObject<Dictionary<string, object>>//new Dictionary<string, object>();

            ////Split request (built by one of our functions) back into parameter dictionary.
            //foreach (string pair in pairs)
            //{
            //    var parts = pair.Split("=".ToCharArray());

            //    if (parts.Count() != 2)
            //    {
            //        Logger.Error($"Tried to resolve a call with a malformed request. Pair: {pair}");
            //        throw new InvalidOperationException();
            //    }

            //    //Parameters are case insensitive; values are the json strings (deserialization comes later)
            //    var key = parts[0].ToLower();
            //    var value = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));

            //    if(parameters.ContainsKey(key))
            //    {
            //        Logger.Warn($"Overwriting existing request key {key}. Original value: {parameters[key]}");
            //        parameters[key] = value;
            //    }
            //    else
            //    {
            //        parameters.Add(key, value);
            //    }
            //}

            Type serviceType = service.GetType();
            var call = DeserializeObject<JsonRemoteCall>(request);
            MethodInfo method = serviceType.GetMethod(call.Method);
            //Dictionary<string, string> preParameters = DeserializeObject<Dictionary<string, string>>(request);
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

            //Look for a matching method.
            //foreach(var method in serviceType.GetMethods())
            //{
            //    var names = method.GetParameters().Select(x => x.Name.ToLower());
            //    var paramKeys = preParameters.Keys.Select(x => x.ToLower());

            //    if(names.IsEquivalentTo(paramKeys))
            //    {
            //        Logger.Debug($"Matched request to method {method.Name}");

            //        var paramValues = new List<object>();

            //        foreach (var param in method.GetParameters())
            //        {
            //            try
            //            {
            //                paramValues.Add(DeserializeGeneral(preParameters[param.Name.ToLower()], param.ParameterType));
            //            }
            //            catch(Exception ex)
            //            {
            //                Logger.Error($"Could not deserialize parameter {param.Name}: {ex}");
            //                throw new InvalidOperationException($"Could not deserialize parameter {param.Name}");
            //            }
            //        }

            //        result = method.Invoke(service, paramValues.ToArray());
            //    }
            //}

            if (result == null)
                return null;
            else
                return SerializeObject(result);
            //throw new NotImplementedException();
        }

        public string CreateCall(MethodInfo info, IEnumerable<object> parameters)//Dictionary<string, object> parameters)
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
            //return SerializeObject(parameters.ToDictionary(x => x.Key, y => SerializeObject(y.Value)));
            //List<string> pairs = new List<string>();

            ////In order to maintain exceptional transport safety (because of proxy bs),
            ////parameters are saved as a query string BUT with the values base64 encoded.
            //foreach (var pair in parameters)
            //{
            //    if(pair.Value is byte[])
            //        pairs.Add($"{pair.Key}={Convert.ToBase64String((byte[])pair.Value)}");
            //    else
            //        pairs.Add($"{pair.Key}={Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SerializeObject(pair.Value)))}");
            //}

            //return string.Join(ParameterSeparator, pairs);
        }
    }

    public class JsonRemoteCall
    {
        public string Method;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
    }
}
