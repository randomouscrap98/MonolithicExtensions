
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Microsoft.Win32;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Windows;
using System.Linq;
using static MonolithicExtensions.Windows.RestartManager;
using MonolithicExtensions.General;
using MonolithicExtensions.Windows.UnitTestSpecial;

namespace MonolithicExtensions.UnitTest
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// These tests require that the IDNExtensionsMonolithicTestService be installed from a specific directory.
    /// This service is NOT required to build MonolithicExtensions; you'll just get some failed unit tests.
    /// This is done so that we can start/stop the service and update the files for the service. As such,
    /// in order for these tests to work, you to make sure that:
    /// 1) IDNExtensionsMonolithicTestService is installed (it's included in this solution; it doesn't have to be running)
    /// 2) The path to the files for the above service is set here.
    /// </remarks>
    [TestClass()]
    public class RestartManagerTest : UnitTestBase
    {
        //If multiple people need to work on this, you can extract this into something outside compilation.
        public const string ServiceDirectory = "..\\..\\..\\MonolithicExtensions.TestService\\bin\\Debug"; 

        //This file must exist in both the test directory AND the service directory and be used by the service.
        public const string ReplacementFile = "log4net.dll";

        public const string ReplacementProcess = "DllBlocker.exe";
        public string ReplacementFileServicePath
        {
            get { return Path.Combine(ServiceDirectory, ReplacementFile); }
        }

        /// <summary>
        /// This ensures that your environment is setup correctly to test the restart manager. If this fails, you should
        /// debug this function to see exactly where things go wrong. If the first assertion fails, that means you need to
        /// install the service. If the next assertion fails, the directory containing the service executable is not where
        /// we expect it to be (in this case, just change the ServiceDirectory variable to point to wherever the service actually is.
        /// </summary>
        [TestMethod()]
        public void TestRestartManagerTestEnvironment()
        {
            var service = ServiceController.GetServices().FirstOrDefault(x => x.ServiceName == TestHelpers.TestService);
            Assert.IsNotNull(service);

            var realServiceDirectory = Path.GetDirectoryName(GetServiceExecutablePath(TestHelpers.TestService));
            var expectedServiceDirectory = Path.GetFullPath(ServiceDirectory);

            Assert.IsTrue(realServiceDirectory.ToLower() == expectedServiceDirectory.ToLower());
            Assert.IsTrue(File.Exists(ReplacementFile));
            Assert.IsTrue(File.Exists(ReplacementFileServicePath));
            Assert.IsTrue(File.Exists(ReplacementProcess));
        }

        /// <summary>
        /// Only see if we can start and stop a restart manager and see the correct service which is locking onto our desired file.
        /// Do NOT actually replace or restart anything.
        /// </summary>
        [TestMethod()]
        public void TestBasicRestartManager()
        {
            //Start up all the little dandies
            StartService(TestHelpers.TestService);
            RestartManagerSession manager = new RestartManagerSession();
            manager.StartSession();
            System.Threading.Thread.Sleep(1000);
            manager.RegisterResources(new List<string>(){ ReplacementFileServicePath });

            //Look for the processes locking on our poor file
            RM_REBOOT_REASON rebootReason = default(RM_REBOOT_REASON);
            var processes = RestartManager.RmProcessToNetProcess(manager.GetList(ref rebootReason));

            //Make sure it's the service we expect.
            var serviceExecutable = GetServiceExecutablePath(TestHelpers.TestService);
            Assert.IsTrue(processes.Count > 0 && processes.Any(x => x.MainModule.FileName.ToLower() == serviceExecutable.ToLower()));

            manager.EndSession();
            StopService(TestHelpers.TestService);
        }

        [TestMethod()]
        public void TestRestartManagerFakeFiles()
        {
            RestartManagerSession manager = new RestartManagerSession();
            manager.StartSession();
            manager.RegisterResources(new List<string>(){ "REALLYNOTAFILE" });

            RM_REBOOT_REASON rebootReason = default(RM_REBOOT_REASON);
            var processes = RestartManager.RmProcessToNetProcess(manager.GetList(ref rebootReason));
            Assert.IsTrue(processes.Count == 0);

            manager.EndSession();
        }

        /// <summary>
        /// NOW that we're pretty sure that the restart manager works, this test makes sure that the service really is holding
        /// onto that file AND that calling shutdown/restart properly lock and unlock the file and all that.
        /// </summary>
        [TestMethod()]
        public void TestRestartManagerServiceFileMove()
        {
            //Start up the service and TRY to move the file. It should fail
            StartService(TestHelpers.TestService);

            //We're hoping this will fail, as the service SHOULD be holding onto this guy
            MyAssert.ThrowsException(() => File.Copy(ReplacementFile, ReplacementFileServicePath, true));

            //Now startup the restart manager and lets hope the service will be restarted.
            RestartManagerSession manager = new RestartManagerSession();
            manager.StartSession();
            manager.RegisterResources(new List<string>(){ ReplacementFileServicePath });

            RM_REBOOT_REASON rebootReason = default(RM_REBOOT_REASON);
            var processes = RestartManager.RmProcessToNetProcess(manager.GetList(ref rebootReason));
            Assert.IsTrue(rebootReason == RM_REBOOT_REASON.RmRebootReasonNone);
            Assert.IsTrue(processes.Count > 0);

            //After shutdown, the file should be copyable
            manager.Shutdown();
            ThreadingServices.WaitOnAction(() => File.Copy(ReplacementFile, ReplacementFileServicePath, true), TimeSpan.FromSeconds(3));

            //Now try to restart everything
            manager.Restart();

            //We're hoping this will fail, as the service SHOULD be holding onto this guy again
            MyAssert.ThrowsException(() => File.Copy(ReplacementFile, ReplacementFileServicePath, true));

            manager.EndSession();
            StopService(TestHelpers.TestService);
        }

        [TestMethod()]
        public void TestRestartManagerProcessFileMove()
        {
            var ReplacementProcessCopy = ReplacementProcess + "2";
            File.Copy(ReplacementProcess, ReplacementProcessCopy, true);

            //Start up the service and TRY to move the file. It should fail
            Process proc = Process.Start(ReplacementProcess);

            //We're hoping this will fail, as the process SHOULD be holding onto this guy
            MyAssert.ThrowsException(() => File.Copy(ReplacementProcessCopy, ReplacementProcess, true));

            //Now startup the restart manager and lets hope the process will be restarted.
            RestartManagerSession manager = new RestartManagerSession();
            manager.StartSession();
            manager.RegisterResources(new List<string>(){ ReplacementProcess });

            RM_REBOOT_REASON rebootReason = default(RM_REBOOT_REASON);
            var processes = RestartManager.RmProcessToNetProcess(manager.GetList(ref rebootReason));
            Assert.IsTrue(rebootReason == RM_REBOOT_REASON.RmRebootReasonNone);
            Assert.IsTrue(processes.Count > 0);

            //After shutdown, the file should be copyable
            manager.Shutdown();
            ThreadingServices.WaitOnAction(() => File.Copy(ReplacementProcessCopy, ReplacementProcess, true), TimeSpan.FromSeconds(3));

            //Now try to restart everything
            manager.Restart();

            manager.EndSession();
        }

        [TestMethod()]
        public void TestRestartManagerManualShutdown()
        {
            var ReplacementProcessCopy = ReplacementProcess + "2";
            File.Copy(ReplacementProcess, ReplacementProcessCopy, true);

            //Start up the service and TRY to move the file. It should fail
            Process proc = Process.Start(ReplacementProcess);

            //We're hoping this will fail, as the process SHOULD be holding onto this guy
            MyAssert.ThrowsException(() => File.Copy(ReplacementProcessCopy, ReplacementProcess, true));

            //Now startup the restart manager and lets hope the process will be restarted.
            RestartManagerExtendedSession manager = new RestartManagerExtendedSession();
            manager.ManualRestartProcesses.Add(ReplacementProcess);
            manager.StartSession();
            manager.RegisterResources(new List<string>(){ ReplacementProcess });

            RM_REBOOT_REASON rebootReason = default(RM_REBOOT_REASON);
            var processes = RestartManager.RmProcessToNetProcess(manager.GetList(ref rebootReason));
            Assert.IsTrue(rebootReason == RM_REBOOT_REASON.RmRebootReasonNone);
            Assert.IsTrue(processes.Count > 0);

            //After shutdown, the file should be copyable
            manager.Shutdown();
            ThreadingServices.WaitOnAction(() => File.Copy(ReplacementProcessCopy, ReplacementProcess, true), TimeSpan.FromSeconds(3));

            //Now try to restart everything
            manager.Restart();

            //We're hoping this will fail, as the service SHOULD be holding onto this guy again
            MyAssert.ThrowsException(() => ThreadingServices.WaitOnAction(() => File.Copy(ReplacementProcessCopy, ReplacementProcess, true), TimeSpan.FromSeconds(2)));

            manager.EndSession();

            System.Diagnostics.Process.GetProcessesByName(ReplacementProcess.Replace(".exe", "")).ToList().ForEach(x => x.Kill());

        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
