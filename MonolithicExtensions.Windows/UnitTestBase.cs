using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.General.Logging;
using MonolithicExtensions.Portable.Logging;
using MonolithicExtensions.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ServiceProcess;
using Microsoft.Win32;

namespace MonolithicExtensions.Windows
{
    public class UnitTestBase
    {
        public ILogger Logger;

        public UnitTestBase(string name = null)
        {
            Log4NetExtensions.SetupDebugLogger("UnitTest");
            Log4NetExtensions.SetLog4NetAsDefaultLogger();
            if (name == null)
                Logger = LogServices.CreateLoggerFromDefault(this.GetType());
            else
                Logger = LogServices.CreateLoggerFromDefault(name);
        }

        public void LogStart(string functionName)
        {
            Logger.Info($"--- Start {functionName} ---");
        }

        public readonly List<int> SimpleList = new List<int>() { 3, 2, 6, 2, 7 };
        public readonly Dictionary<int, string> SimpleDictionary = new Dictionary<int, string>()
        {
            {2, "heh two"},
            {22, "heh another two"},
            {3, "wow three so original"},
            {6, "skipped a few numbers there"},
            {7, "lucky"}
        };

        public readonly ComplexContainer SimpleClass = new ComplexContainer() { Description = "Just a simple class" };
        public readonly InheritedContainer SimpleInheritedClass = new InheritedContainer() { Description = "A simple inherited container", SelfIdentifier = "INHERITANCE" };

        public readonly List<ComplexContainer> ComplexClassList = new List<ComplexContainer>()
        {
            new ComplexContainer() {Description = "First container"},
            new ComplexContainer() {Description = "secund contaner"},
            new ComplexContainer() {Description = "thurd contnr"},
            new ComplexContainer() {Description = "furht aontcen"}
        };

        private readonly List<ServiceControllerStatus> ServiceStartedStatuses = new List<ServiceControllerStatus>() { ServiceControllerStatus.Running, ServiceControllerStatus.StartPending };

        /// <summary>
        /// An easy way to ensure proper startup of the given service name (for unit tests, anyway)
        /// </summary>
        /// <param name="ServiceName"></param>
        public void StartService(string ServiceName)
        {
            using (ServiceController controller = new ServiceController(ServiceName))
            {
                if (!ServiceStartedStatuses.Contains(controller.Status))
                {
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
            }
        }

        /// <summary>
        /// An easy way to ensure proper shutdown of the given service name (for unit tests, anyway)
        /// </summary>
        /// <param name="ServiceName"></param>
        public void StopService(string ServiceName)
        {
            using (ServiceController controller = new ServiceController(ServiceName))
            {
                if (ServiceStartedStatuses.Contains(controller.Status))
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }
            }
        }

        /// <summary>
        /// An easy way to retrieve the apparent file path for the executable that is the given service.
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns></returns>
        public string GetServiceExecutablePath(string ServiceName)
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + ServiceName))
            {
                Assert.IsNotNull(key);
                dynamic servicePath = key.GetValue("ImagePath");
                Assert.IsNotNull(servicePath);
                return servicePath.ToString().Replace("\"", "");
            }
        }
    }

    public static class MyAssert
    {
        /// <summary>
        /// Throw an assertion failure if the given action does not throw an exception
        /// </summary>
        /// <param name="FailAction"></param>
        public static void ThrowsException(Action FailAction)
        {
            bool threwException = false;
            try
            {
                FailAction.Invoke();
            }
            catch //(Exception ex)
            {
                threwException = true;
            }

            if (!threwException)
                Assert.Fail();
        }
    }

    /// <summary>
    /// Just a container with many fields to help test out various extensions. Isn't actually useful for anything.
    /// </summary>
    public class ComplexContainer
    {
        //It's nice to have readonly values
        public Guid ID { get; }
        public DateTime CreateTime { get; }

        //A tricky readonly value that isn't an automatic property
        public double Secrets
        {
            get { return SecretValue; }
        }

        //And some publicly settable garbage
        public string Description { get; set; }
        public List<ComplexContainer> References { get; set; }

        //And finally a few hidden things just for good measure
        protected Dictionary<int, string> FileMapping { get; set; }
        private double SecretValue { get; set; }

        //There's no value that must be set on construction, so here's a nice parameterless constructor for you
        public ComplexContainer()
        {
            ID = Guid.NewGuid();
            CreateTime = DateTime.Now;
            Clear();
        }

        //It's nice to have a copy constructor, just in case some test needs it.
        public ComplexContainer(ComplexContainer CopyObject)
        {
            ID = CopyObject.ID;
            CreateTime = CopyObject.CreateTime;
            Description = CopyObject.Description;
            References = new List<ComplexContainer>(CopyObject.References);
            FileMapping = new Dictionary<int, string>(CopyObject.FileMapping);
            SecretValue = CopyObject.SecretValue;
        }

        /// <summary>
        /// Completely clear out all data except ID and CreateTime
        /// </summary>
        public void Clear()
        {
            Description = "";
            References = new List<ComplexContainer>();
            FileMapping = new Dictionary<int, string>();
            SecretValue = 0;
        }

        public bool AddMapping(int ID, string Path)
        {
            if (FileMapping.ContainsKey(ID))
                return false;
            FileMapping.Add(ID, Path);
            return true;
        }

        public string GetPathByID(int ID)
        {
            if (FileMapping.ContainsKey(ID))
                return FileMapping[ID];
            else
                return null;
        }

        public void PerformSecrets()
        {
            SecretValue = (DateTime.Now.AddDays(-439820).Ticks % 100000) / 39857;
        }

        public override bool Equals(object obj)
        {
            ComplexContainer testObject = null;

            try
            {
                testObject = (ComplexContainer)obj;
            }
            catch //(Exception ex)
            {
                return false;
            }

            return ID == testObject.ID & Description == testObject.Description & References.Select(x => x.ID).SequenceEqual(testObject.References.Select(x => x.ID)) & SecretValue == testObject.SecretValue & CreateTime == testObject.CreateTime & FileMapping.IsEquivalentTo(testObject.FileMapping);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class InheritedContainer : ComplexContainer
    {
        public string SelfIdentifier { get; set; }

        public override bool Equals(object obj)
        {
            InheritedContainer testObject = null;

            try
            {
                testObject = (InheritedContainer)obj;
            }
            catch //(Exception ex)
            {
                return false;
            }

            return base.Equals(obj) && SelfIdentifier == testObject.SelfIdentifier;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
