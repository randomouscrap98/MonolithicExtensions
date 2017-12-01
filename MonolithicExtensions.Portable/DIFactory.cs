using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonolithicExtensions.Portable
{
    /// <summary>
    /// Allows you to define the methods used to create your objects.
    /// <remark>The type mappings you supply should always be mapped by interface and return interfaces. 
    /// Nothing in this class should know about your underlying objects.</remark>
    /// </summary>
    public class DIFactory
    {
        protected Logging.ILogger Logger = null;

        public Dictionary<Type, Func<DIFactory, object>> CreationMapping { get; set; } 
        public Dictionary<Type, Action<DIFactory, object>> ReleaseMapping { get; set; }
        protected Dictionary<string, object> Settings { get; set; }

        public DIFactory(bool setupLogger = true)
        {
            CreationMapping = new Dictionary<Type, Func<DIFactory, object>>();
            ReleaseMapping = new Dictionary<Type, Action<DIFactory, object>>();
            Settings = new Dictionary<string, object>();

            if (setupLogger)
                SetupLogger();
        }

        public DIFactory(DIFactory copy, bool setupLogger = true) : this(setupLogger)
        {
            CreationMapping = new Dictionary<Type, Func<DIFactory, object>>(copy.CreationMapping);
            ReleaseMapping = new Dictionary<Type, Action<DIFactory, object>>(copy.ReleaseMapping);
            Settings = new Dictionary<string, object>(copy.Settings);
        }

        /// <summary>
        /// The logger must be set up as late as possible (since the logsystem uses US), so 
        /// </summary>
        public void SetupLogger()
        {
            if(Logger == null)
                Logger = Logging.LogServices.CreateLoggerFromDefault(this.GetType());
        }

        /// <summary>
        /// Retrieve a setting by using an object as a key rather than a string.
        /// This may be used to index settings with an enum rather than a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="mapObject"></param>
        /// <returns></returns>
        public V GetSetting<T, V>(T mapObject)
        {
            return (V)Settings[mapObject.ToString()];
        }

        /// <summary>
        /// Retrieve a single setting using the type of the desired result as the key.
        /// <remarks>If you KNOW that there is only ONE of your type of setting in the settings array,
        /// you should just access it by type with this function.</remarks>
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public V GetSettingByType<V>()
        {
            return GetSetting<Type, V>(typeof(V));
        }

        /// <summary>
        /// Set a setting by using an object as a key rather than a string. 
        /// This may be used to index settings with an enum rather than a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapObject"></param>
        /// <param name="value"></param>
        public void SetSetting<T>(T mapObject, object value)
        {
            string key = mapObject.ToString();
            if (!Settings.ContainsKey(key))
            {
                if(Logger != null)
                    Logger.Debug("Creating setting in DI container: " + key);
                Settings.Add(key, null);
            }
            Settings[key] = value;
        }

        /// <summary>
        /// Set a single setting using the type of the inserted value as the key.
        /// <remarks>If you KNOW that there is only ONE of your type of setting in the settings array,
        /// you should just access it by type with this function.</remarks>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void SetSettingByType<T>(T value)
        {
            SetSetting<Type>(typeof(T), value);
        }

        public object Create(Type objectType)
        {
            if(Logger != null)
                Logger.Trace("Attempting to create object of type " + objectType.Name);
            if (!CreationMapping.ContainsKey(objectType))
            {
                throw new InvalidOperationException("There is no method to create this type!");
            }
            return CreationMapping[objectType].Invoke(this);
        }

        /// <summary>
        /// Attempt to create an interface/service/whatever of the given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Create<T>()
        {
            return (T)Create(typeof(T));
        }

        /// <summary>
        /// Attempt to release the given object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldObject"></param>
        public void Release<T>(T oldObject)
        {
            Type type = typeof(T);
            if(Logger != null)
                Logger.Trace("Attempting to release object of type " + type.Name);
            if (ReleaseMapping.ContainsKey(type))
            {
                ReleaseMapping[type].Invoke(this, oldObject);
            }
            else
            {
                if(Logger != null)
                    Logger.Debug("There is no method to release objects of type " + type.Name + ". Ignoring.");
            }
        }

        /// <summary>
        /// Adopt the given new factory's create/release routines and settings and merge it with our own. 
        /// Fails with InvalidOperationException if there are duplicate definitions.
        /// </summary>
        /// <param name="newFactory"></param>

        public void MergeWithSelf(DIFactory newFactory)
        {
            //For each container, merge the creation mappings
            foreach (var cMapping in newFactory.CreationMapping)
            {
                if (CreationMapping.ContainsKey(cMapping.Key))
                {
                    throw new InvalidOperationException("Duplicate methods for creating type " + cMapping.Key.ToString());
                }
                else
                {
                    CreationMapping.Add(cMapping.Key, cMapping.Value);
                }
            }

            //For each container, merge the release mappings
            foreach (var rMapping in newFactory.ReleaseMapping)
            {
                if (ReleaseMapping.ContainsKey(rMapping.Key))
                {
                    throw new InvalidOperationException("Duplicate methods for releasing type " + rMapping.Key.ToString());
                }
                else
                {
                    ReleaseMapping.Add(rMapping.Key, rMapping.Value);
                }
            }

            //Finally, for each container, merge the settings.
            foreach (var setting in newFactory.Settings)
            {
                if (Settings.ContainsKey(setting.Key))
                {
                    throw new InvalidOperationException("Duplicate settings key: " + setting.Key);
                }
                else
                {
                    Settings.Add(setting.Key, setting.Value);
                }
            }

        }

        /// <summary>
        /// Merge multiple DI Factories into a single factory. If settings or object mappings collide, it will throw
        /// an InvalidOperation exception.
        /// </summary>
        /// <param name="originalContainer"></param>
        /// <param name="extraContainers"></param>
        /// <returns></returns>
        public static DIFactory Merge(DIFactory originalContainer, params DIFactory[] extraContainers)
        {
            DIFactory newFactory = new DIFactory(originalContainer);

            foreach (var container in extraContainers)
            {
                newFactory.MergeWithSelf(container);
            }

            return newFactory;
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
