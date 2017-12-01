using MonolithicExtensions.General.Logging;
using MonolithicExtensions.Portable.Logging;
using MonolithicExtensions.Windows.UnitTestSpecial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonolithicExtensions.TestService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
            Log4NetExtensions.SetupDebugLogger("TestService");
            Log4NetExtensions.SetLog4NetAsDefaultLogger();
            Logger = LogServices.CreateLoggerFromDefault(this.GetType());
        }

        ServiceHost ExtensionsTestService = null;
        Task runningTask = null;
        CancellationTokenSource cancelSource = new CancellationTokenSource();
        private ILogger Logger { get; }

        protected override void OnStart(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            var token = cancelSource.Token;
            runningTask = Task.Factory.StartNew(() => MainTask(token), token);

            try
            {
                ExtensionsTestService = new ServiceHost(typeof(SpecialTestService), new Uri(TestHelpers.ExtensionsTestAddress));
                var binding = new NetNamedPipeBinding();
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.MaxConnections = int.MaxValue;
                ExtensionsTestService.AddServiceEndpoint(typeof(ISpecialTestContract), binding, TestHelpers.ExtensionsServiceName);
                var behavior = ExtensionsTestService.Description.Behaviors.Find<ServiceBehaviorAttribute>();
                behavior.InstanceContextMode = InstanceContextMode.PerSession;
                behavior.ConcurrencyMode = ConcurrencyMode.Single;
                ExtensionsTestService.Open();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception while starting ExtensionsTest WCF service: " + ex.ToString());
            }
        }

        protected void MainTask(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                Logger.Info("IDN.Extensions.Monolithic.TestService is still running!");
                System.Threading.Thread.Sleep(1000);
            }
        }

        protected override void OnStop()
        {
            if (runningTask != null)
            {
                cancelSource.Cancel();
                runningTask.Wait();
            }
            if (ExtensionsTestService != null)
            {
                try
                {
                    ExtensionsTestService.Close();
                    ExtensionsTestService = null;
                }
                catch (Exception ex)
                {
                    Logger.Error("Could not close ExtensionsTest WCF service: " + ex.ToString());
                }
            }
        }

    }
}
