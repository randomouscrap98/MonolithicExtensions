using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.General;
using MonolithicExtensions.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableExtensions.UnitTest
{
    [TestClass]
    public class RemoteCallTests : UnitTestBase
    {
        public RemoteCallTests() : base("RemoteCallTests") { }

        [TestMethod]
        public void TestBasicJsonRemoteCall()
        {
            LogStart("TestBasicJsonRemoteCall");

            var service = new SimpleService();
            var serviceType = service.GetType();
            var remote = new JsonRemoteCallService();

            string call = remote.CreateCall(serviceType.GetMethod("AddNumbers"), new List<object> { 5, 7 });
            var result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<int>(result) == 12);

            call = remote.CreateCall(serviceType.GetMethod("MultiplyNumbers"), new List<object> { 5, 8 });
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<int>(result) == 40);

            call = remote.CreateCall(serviceType.GetMethod("GetHello"), null);
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<string>(result) == "Hello!");
        }
    }

    public class SimpleService
    {
        public int AddNumbers(int a, int b)
        {
            return a + b;
        }

        public long MultiplyNumbers(long a, long b)
        {
            return a * b;
        }

        public string GetHello()
        {
            return "Hello!";
        }
    }
}
