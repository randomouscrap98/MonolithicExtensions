
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Windows;
using MonolithicExtensions.Windows.UnitTestSpecial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class WindowsAccountsTest : UnitTestBase
    {
        ChannelFactory<ISpecialTestContract> channelFactory;
        ISpecialTestContract testService;

        [TestInitialize()]
        public void PreTestSetup()
        {
            try
            {
                var binding = new NetNamedPipeBinding();
                binding.MaxReceivedMessageSize = int.MaxValue;
                binding.MaxBufferSize = 256000;
                channelFactory = new ChannelFactory<ISpecialTestContract>(binding, new EndpointAddress(TestHelpers.ExtensionsTestAddress + TestHelpers.ExtensionsServiceName));
                testService = channelFactory.CreateChannel();
                StartService(TestHelpers.TestService);
            }
            catch (Exception ex)
            {
                Logger.Error("Could not setup test environment for WindowsAccountsTest: " + ex.ToString());
            }

        }

        [TestMethod()]
        public void TestSimpleAccountTokenRetrieval()
        {
            var result = testService.GetCurrentUserToken();
            var trueResult = result.GetResult();
            Assert.IsTrue(trueResult);
        }

        [TestMethod()]
        public void TestUserTokenFromProcessRetrieval()
        {
            var result = testService.GetExplorerUserToken();
            var trueResult = result.GetResult();
            Assert.IsTrue(trueResult);
        }

        [TestMethod()]
        public void TestStartProcessByUser()
        {
            //NOTE: You MUST change this in order for it to work on other machines! This is just a holdover
            var result = testService.StartProcessAsCurrentUser("IDNETWORKS\\csanchez");
            var trueResult = result.GetResult();
            Assert.IsTrue(trueResult);
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            try
            {
                channelFactory.Close();
                StopService(TestHelpers.TestService);
            }
            catch (Exception ex)
            {
                Logger.Error("Could not cleanup test environment for WindowsAccountsTest: " + ex.ToString());
            }
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
