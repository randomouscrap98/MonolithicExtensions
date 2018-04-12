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

        public const string RemoteCallService = "unittestremotecall";

        public GeneralRemoteCallConfig CreateGeneralConfig()
        {
            return new GeneralRemoteCallConfig() { LowLevelLogging = true };
        }

        public JsonRemoteCallService CreateJsonService()
        {
            return new JsonRemoteCallService(CreateGeneralConfig());
        }

        [TestMethod]
        public void TestBasicJsonRemoteCall()
        {
            LogStart("TestBasicJsonRemoteCall");

            var service = new SimpleService();
            var serviceType = service.GetType();
            var remote = CreateJsonService();

            string call = remote.CreateCall(serviceType.GetMethod("AddNumbers"), new List<object> { 5, 7 });
            var result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<int>(result) == 12);

            call = remote.CreateCall(serviceType.GetMethod("MultiplyNumbers"), new List<object> { 5, 8 });
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<int>(result) == 40);

            call = remote.CreateCall(serviceType.GetMethod("GetHello"), null);
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<string>(result) == "Hello!");

            call = remote.CreateCall(serviceType.GetMethod("SetHello"), new List<object> { "Banana" });
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(result == null);

            call = remote.CreateCall(serviceType.GetMethod("GetHello"), null);
            result = remote.ResolveCall(call, service);
            Assert.IsTrue(remote.DeserializeObject<string>(result) == "Banana");
        }

        [TestMethod]
        public void TestHttpRemoteCall()
        {
            LogStart("TestHttpRemoteCall");

            var serverService = new SimpleService();
            var serviceType = typeof(SimpleService);

            var server = new HttpRemoteCallServer(CreateJsonService(), new HttpRemoteCallServerConfig(), CreateGeneralConfig());
            var client = new HttpRemoteCallClient(CreateJsonService(), new HttpRemoteCallClientConfig());

            server.Start("http://+:45677", new Dictionary<string, object>() { { RemoteCallService, serverService } });

            client.Endpoint = $"http://localhost:45677/{RemoteCallService}";
            int result = client.CallAsync<int>(serviceType.GetMethod("AddNumbers"), new List<object>() { 5, 6 }).Result;
            Assert.IsTrue(result == 11);
            result = client.Call<int>(serviceType.GetMethod("AddNumbers"), new List<object>() { 5, 6 });
            Assert.IsTrue(result == 11);

            result = (int)client.CallAsync<long>(serviceType.GetMethod("MultiplyNumbers"), new List<object>() { 7, 8 }).Result;
            Assert.IsTrue(result == 56);
            result = (int)client.Call<long>(serviceType.GetMethod("MultiplyNumbers"), new List<object>() { 7, 8 });
            Assert.IsTrue(result == 56);

            Assert.IsTrue(client.CallAsync<string>(serviceType.GetMethod("GetHello"), null).Result == "Hello!");
            Assert.IsTrue(client.Call<string>(serviceType.GetMethod("GetHello"), null) == "Hello!");
            client.CallVoidAsync(serviceType.GetMethod("SetHello"), new List<object>() { "doggo" }).Wait();
            Assert.IsTrue(client.CallAsync<string>(serviceType.GetMethod("GetHello"), null).Result == "doggo");

            client.CallVoidAsync(serviceType.GetMethod("SetHello"), new List<object>() { null }).Wait();
            Assert.IsTrue(client.CallAsync<string>(serviceType.GetMethod("GetHello"), null).Result == null);
            client.CallVoidAsync(serviceType.GetMethod("SetHello"), new List<object>() { "newstring" }).Wait();
            Assert.IsTrue(client.CallAsync<string>(serviceType.GetMethod("GetHello"), null).Result == "newstring");

            MyAssert.ThrowsException(() => client.CallVoidAsync(serviceType.GetMethod("ThrowException"), null).Wait());
            MyAssert.ThrowsException(() => client.CallVoid(serviceType.GetMethod("ThrowException"), null));
            server.Stop();
        }
    }

    public class SimpleService
    {
        private string hello = "Hello!";

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
            return hello;
        }

        public void SetHello(string hello)
        {
            this.hello = hello;
        }

        public void ThrowException()
        {
            throw new InvalidOperationException("LOL something");
        }
    }
}
