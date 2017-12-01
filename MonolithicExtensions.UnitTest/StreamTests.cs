using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Portable;
using MonolithicExtensions.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableExtensions.UnitTest
{
    [TestClass]
    public class StreamTests : UnitTestBase 
    {
        public StreamTests() : base("UnitTestBase") { }

        [TestMethod]
        public void TestDoubleStream()
        {
            byte[] inputBytes = new byte[] { 6, 8, 4, 22, 11 };
            byte[] outputBytes = new byte[] { 66, 88, 101 };

            MemoryStream inputStream = new MemoryStream(inputBytes);
            MemoryStream outputStream = new MemoryStream();
            DualStream testStream = new DualStream();
            testStream.InputStream = inputStream;
            testStream.OutputStream = outputStream;

            foreach (byte value in inputBytes)
                Assert.IsTrue(testStream.ReadByte() == value);

            foreach (byte value in outputBytes)
                testStream.WriteByte(value);

            Assert.IsTrue(outputStream.ToArray().SequenceEqual(outputBytes));
            Assert.IsTrue(testStream.Read(inputBytes, 0, 5) == 0);
        }
    }
}
