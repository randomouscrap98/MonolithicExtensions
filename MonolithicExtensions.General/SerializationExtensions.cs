using log4net;
using MonolithicExtensions.Portable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonolithicExtensions.General
{
    public static class MySerialize
    {
        //This JSON serialization thing requires global settings.... great. These are the settings.
        private static JsonSerializerSettings defaultSettings { get; } = new JsonSerializerSettings()
        {
            ContractResolver = new MyContractResolver(),
            Formatting = Formatting.None,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        /// <summary>
        /// Save the given object to the given TextWriter stream. This is the lowest level way to save an object; you probably
        /// want something else, like SaveObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="saveObject"></param>
        /// <param name="stream"></param>
        /// <param name="expanded"></param>
        public static void JsonStringifyToStream<T>(T saveObject, ref TextWriter stream, bool expanded = false)
        {
            JsonConvert.DefaultSettings = () => defaultSettings;

            var serializer = JsonSerializer.CreateDefault();

            //IDK, maybe people don't want formatted json. Maybe they're crazy.
            if ((expanded))
                serializer.Formatting = Formatting.Indented;
            else
                serializer.Formatting = Formatting.None;

            serializer.Serialize(stream, saveObject);
        }

        /// <summary>
        /// Convert given object to JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="convertObject"></param>
        /// <param name="expanded"></param>
        /// <returns></returns>
        public static string JsonStringify<T>(T convertObject, bool expanded = false)
        {
            TextWriter stream = new StringWriter();
            JsonStringifyToStream(convertObject, ref stream, expanded);
            string result = stream.ToString();
            stream.Dispose();
            return result;
        }

        /// <summary>
        /// Parse a json string into an object of the desired type. This is the lowest level parsing call; you probably want something like
        /// LoadObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T JsonParseFromStream<T>(ref TextReader stream)
        {
            JsonConvert.DefaultSettings = () => defaultSettings;
            T newObject = default(T);

            var serializer = JsonSerializer.CreateDefault();
            newObject = (T)serializer.Deserialize(stream, typeof(T));

            return newObject;
        }

        /// <summary>
        /// Convert the given json string into the desired object (if possible)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T JsonParse<T>(string jsonString)
        {
            TextReader stream = new StringReader(jsonString);
            T result = JsonParseFromStream<T>(ref stream);
            stream.Dispose();
            return result; 
        }

        /// <summary>
        /// Save the given object as a JSON file with the given filename.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        /// <param name="saveObject"></param>
        /// <param name="expanded"></param>
        public static void SaveObject<T>(string filename, T saveObject, bool expanded = false)
        {
            string objectDirectory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(objectDirectory))
                Directory.CreateDirectory(objectDirectory);

            TextWriter filestream = File.CreateText(filename);
            JsonStringifyToStream(saveObject, ref filestream, expanded);
            filestream.Dispose();
        }

        /// <summary>
        /// Load an object from the given JSON file. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filename"></param>
        public static T LoadObject<T>(string filename)
        {
            TextReader filestream = File.OpenText(filename);
            T result = JsonParseFromStream<T>(ref filestream);
            filestream.Dispose();
            return result;
        }

        //Taken from http//stackoverflow.com/questions/24106986/json-net-force-serialization-of-all-private-fields-And-all-fields-in-sub-classe
        public class MyContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                JsonContract contract = base.CreateContract(objectType);

                //This is just so that versions get automatically serialized correctly. Not sure why they're not already... honestly.
                if (objectType == typeof(Version))
                    contract.Converter = new VersionConverter();

                return contract;
            }

            #region "GarbageAttempts"
            //Protected Overrides Function CreateProperty(member As MemberInfo, memberSerialization As MemberSerialization) As JsonProperty

            //    Dim prop = MyBase.CreateProperty(member, memberSerialization)
            //    prop.Writable = ShouldSerialize(member, True) 'CanSetMemberValue(member, True)
            //    prop.Readable = ShouldSerialize(member, True) 'CanReadMemberValue(member, True)
            //    prop.Ignored = Not ShouldSerialize(member, True)
            //    Return prop

            //End Function

            //Private Function ShouldSerialize(Member As MemberInfo, NonPublic As Boolean) As Boolean

            //    'Return True
            //    Select Case (Member.MemberType)
            //        Case MemberTypes.Field 'ALL fields should be serialized.
            //            Return True
            //            'Dim fInfo = CType(Member, FieldInfo)
            //            'Return NonPublic Or fInfo.IsPublic
            //        Case MemberTypes.Property
            //            Dim pInfo = CType(Member, PropertyInfo)
            //            Return pInfo.GetMethod = Nothing OrElse pInfo.SetMethod <> Nothing
            //            'If Not PropertyInfo.CanWrite Then Return False
            //            'If NonPublic Then Return True
            //            'Return PropertyInfo.GetSetMethod(NonPublic) <> Nothing
            //        Case Else
            //            Return False
            //    End Select

            //End Function

            //Private Function CanSetMemberValue(Member As MemberInfo, NonPublic As Boolean) As Boolean

            //    Select Case (Member.MemberType)
            //        Case MemberTypes.Field
            //            Dim FieldInfo = CType(Member, FieldInfo)
            //            Return NonPublic Or FieldInfo.IsPublic
            //        Case MemberTypes.Property
            //            Dim PropertyInfo = CType(Member, PropertyInfo)
            //            If Not PropertyInfo.CanWrite Then Return False
            //            If NonPublic Then Return True
            //            Return PropertyInfo.GetSetMethod(NonPublic) <> Nothing
            //        Case Else
            //            Return False
            //    End Select

            //End Function

            //Protected Overrides Function CreateProperties(type As Type, memberSerialization As MemberSerialization) As IList(Of JsonProperty)

            //    Dim props = MyBase.CreateProperties(type, memberSerialization)

            //    'SERIALIZE EVERYTHING OMG
            //    'For Each prop In props
            //    '    prop.Ignored = False
            //    '    prop.Readable = True
            //    'Next

            //    'Everything that is NOT readonly should not be ignored when serializing
            //    'For Each prop In props.Where(Function(x) Not (x.Readable And Not x.Writable))
            //    '    prop.Ignored = False
            //    'Next

            //    Return props

            //End Function

            #endregion

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                //Dim properties = type.GetProperties(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance).Select(
                //   Function(p) MyBase.CreateProperty(p, memberSerialization))

                Type currentType = type;
                List<JsonProperty> fields = new List<JsonProperty>();

                //Walk inheritance tree to get ALL values in ALL types. Yeah
                while (currentType != null)
                {
                    fields.AddRange(currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => !typeof(ILog).IsAssignableFrom(f.FieldType) && !typeof(MonolithicExtensions.Portable.Logging.ILogger).IsAssignableFrom(f.FieldType))
                        .Select(f => base.CreateProperty(f, memberSerialization)));
                    currentType = currentType.BaseType;
                }

                var props = fields.GroupBy(x => x.PropertyName).Select(x => x.First()).ToList();
                //properties.Union(fields).ToList()

                props.ForEach(p =>
                {
                    p.Writable = true;
                    p.Readable = true;
                });
                return props;
            }
        }
    }

    /// <summary>
    /// Allows easy transport of complex objects over communication pathways where serialization is stupid. For instance, transfering a
    /// complex object over WCF may fail. Use this to wrap your object!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Runtime.Serialization.DataContract()]
    public class JsonTransporter<T>
    {
        //If having a string causes issues, change to byte array.
        [System.Runtime.Serialization.DataMember()]
        private string data = "";

        public JsonTransporter()
        {
        }

        public JsonTransporter(T transportObject)
        {
            SetObject(transportObject);
        }

        public void SetObject(T transportObject)
        {
            data = MySerialize.JsonStringify(transportObject, false);
        }

        public T GetObject()
        {
            return MySerialize.JsonParse<T>(data);
        }
    }

    /// <summary>
    /// Extension functions for the JsonTransporter class
    /// </summary>
    public static class JsonTransporter
    {
        /// <summary>
        /// Allows simpler creation of JsonTransport objects. Does the same thing as passing the given object
        /// to the regular JsonTransporter constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transportObject"></param>
        /// <returns></returns>
        public static JsonTransporter<T> Create<T>(T transportObject)
        {
            return new JsonTransporter<T>(transportObject);
        }
    }

    /// <summary>
    /// Combines the CapturedExceptionResult and JsonTransporter classes into a single, easier to use
    /// (although less featureful) class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Runtime.Serialization.DataContract()]
    public class CapturedExceptionResultTransporter<T> : JsonTransporter<CapturedExceptionResult<T>>
    {
        public CapturedExceptionResultTransporter(Exception ex) : base(new CapturedExceptionResult<T>(ex))
        {
        }

        public CapturedExceptionResultTransporter(T result, Exception ex = null) : base(new CapturedExceptionResult<T>(result, ex))
        {
        }

        /// <summary>
        /// Returns result, but throws stored exception if there is one.
        /// </summary>
        /// <returns></returns>
        public T GetResult()
        {
            var data = GetObject();
            if (data.HasException)
                throw data.ThrownException;
            else
                return data.Result;
        }
    }
}
