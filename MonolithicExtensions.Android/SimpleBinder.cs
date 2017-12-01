using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MonolithicExtensions.Portable.Logging;

namespace MonolithicExtensions.Android
{
    //The classes in this file provide an easy method for creating and binding to Android services. If you
    //derive your services from the SimpleService class, then bind using the SimpleBinding class, you can
    //bypass the need for custom ServiceConnector and IBinder classes. 

    /// <summary>
    /// An IBinder that simply has a reference to a service. Used in the SimpleService implementation alongside
    /// SimpleBinding to support easy binding.
    /// </summary>
    public class SimpleBinder : Binder
    {
        public Service BoundService;
    }

    /// <summary>
    /// Derive from this class to allow simple binding with a redirect back to the service itself. 
    /// Works hand-in-hand with SimpleBinding to wrap the horrible Android service binding process
    /// </summary>
    public class SimpleService : Service
    {
        protected SimpleBinder bindRedirector;
        protected ILogger Logger;
        protected Handler uiHandler;

        public SimpleService()
        {
            bindRedirector = new SimpleBinder() { BoundService = this };
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
            uiHandler = new Handler();
        }

        public override IBinder OnBind(Intent intent)
        {
            Logger.Debug($"SimpleService of type {this.GetType()} bound!");
            return bindRedirector;
        }

        public void RunOnUiThread(Action action)
        {
            uiHandler.Post(action);
        }

        public void DoToast(string message, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, message, length).Show());
        }

        public void DoToast(int messageResource, ToastLength length = ToastLength.Short)
        {
            RunOnUiThread(() => Toast.MakeText(this, messageResource, length).Show());
        }
    }

    /// <summary>
    /// Wraps the nonsense required just to bind to a simple service. Works ONLY with the SimpleService
    /// and SimpleBinding classes. You will not need a custom ServiceConnection 
    /// implementation per-activity; simply instantiate this object and use its events.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleBinding<T> : EventServiceConnection<T> where T : SimpleService
    {
        public SimpleBinding()
        {
            ServiceCastFunction = (b) => (T)((SimpleBinder)b).BoundService;
        }

        /// <summary>
        /// Wraps the bind process by calling BindService on the given context. Will invoke the various
        /// events and set the connection state and service object as they become available.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bindType"></param>
        /// <param name="customIntent"></param>
        /// <returns></returns>
        public bool SimpleBind(Activity context, Bind bindType = Bind.AutoCreate, Intent customIntent = null)
        {
            Logger.Trace($"SimpleBind called for service type {typeof(T)}");

            if(customIntent == null)
                customIntent = new Intent(context, typeof(T));

            if (context.BindService(customIntent, this, bindType))
            {
                return true;
            }
            else
            {
                Logger.Error($"Could not bind to android service of type {typeof(T)}!");
                return false;
            }
        }

        /// <summary>
        /// Only unbind the service if it's already bound. Returns whether or not unbindService was called.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool TryUnbind(Context context)
        {
            Logger.Trace($"TryUnbind called for service type {typeof(T)}");

            if(Connected)
            {
                context.UnbindService(this);
                ResetState(); //Reset the connection state since we're technically not bound anymore.
                return true;
            }
            else
            {
                Logger.Debug($"Tried to unbind, but the {typeof(T)} service isn't bound");
                return false;
            }
        }
    }

    /// <summary>
    /// Allows you to work with binding to services using proper .NET events rather than that other crap
    /// they think is good. Also keeps track of connection state and bound service for you. Assumes that the
    /// IBinder returned by the android system IS the service interface!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventServiceConnection<T> : Java.Lang.Object, IServiceConnection
    {
        public event Action<T> ServiceConnected;
        public event Action ServiceDisconnected;
        public Func<IBinder, T> ServiceCastFunction;

        private bool _Connected;
        private T _Service;

        protected ILogger Logger;

        public EventServiceConnection()
        {
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
            ResetState();
            ServiceCastFunction = (b) => (T)b;
        }

        protected void ResetState()
        {
            Logger.Trace($"Reset State for simple binder of type {typeof(T)}");
            _Connected = false;
            _Service = default(T);
        }

        public bool Connected
        {
            get { return _Connected; }
        }

        public T Service
        {
            get { return _Service; }
        }

        /// <summary>
        /// This is the android-required function for this interface. You do NOT need to call or do anything
        /// with this function; use the ServiceConnected event instead.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="service"></param>
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Logger.Trace($"Android OnServiceConnected for {service.GetType()}");

            try { _Service = ServiceCastFunction(service); }
            catch(Exception E) { Logger.Error($"Couldn't cast service: {E}"); }
            _Connected = true;

            Action<T> handler = ServiceConnected;

            if (handler != null)
            {
                handler(_Service);
            }
            else
            {
                Logger.Debug("No handler(s) for ServiceConnected event");
            }
        }

        /// <summary>
        /// This is the android-required function for this interface. You do NOT need to call or do anything
        /// with this function; use the ServiceDisconnected event instead.
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {
            Logger.Trace($"Android OnServiceDisconnected for {name}");

            ResetState(); //This might need to go after the handler?
            Action handler = ServiceDisconnected;

            if (handler != null)
            {
                handler();
            }
            else
            {
                Logger.Debug("No handler(s) for ServiceDisconnected event");
            }
        }
    }

}