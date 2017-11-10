using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Portable;
using MonolithicExtensions.Windows;

namespace MonolithicExtensions.UnitTest
{
    [TestClass]
    public class BasicTypesTest : UnitTestBase
    {
        public BasicTypesTest() : base("BasicTypesTest") { }

        [TestMethod]
        public void TestCamelCase()
        {
            LogStart("TestCamelCase");
            var newString = "ThisIsMyCamelCase".CamelCaseToSpaced();
            Assert.IsTrue(newString == "This Is My Camel Case");

            newString = "GetJSONObject".CamelCaseToSpaced();
            Assert.IsTrue(newString == "Get JSONObject");

            newString = "Single";
            Assert.IsTrue(newString == "Single");
        }

        [TestMethod] public void TestStringToBoolean()
        {
            LogStart("TestStringToBoolean");

            Assert.IsTrue("true".ToBoolean());
            Assert.IsFalse("false".ToBoolean());
            Assert.IsTrue("t".ToBoolean());
            Assert.IsFalse("f".ToBoolean());
            Assert.IsTrue("1".ToBoolean());
            Assert.IsFalse("0".ToBoolean());
            Assert.IsFalse("-1".ToBoolean());
            Assert.IsTrue("-1".ToBoolean(true));
            Assert.IsTrue("yes".ToBoolean());
            Assert.IsTrue("y".ToBoolean());
            Assert.IsFalse("no".ToBoolean());
            Assert.IsFalse("n".ToBoolean());
        }

        [TestMethod]
        public void TestIntToGuid()
        {
            LogStart("TestIntToGuid");

            var previousGuid = Guid.Empty;

            //Not much we can do beyond just making sure it generates the same guid for the same values and different guids for different values.
            for (int i = 0; i < 100; i++)
            {
                var thisGuid = i.ToGuid();
                if (i > 0)
                {
                    Assert.IsTrue(previousGuid == (i - 1).ToGuid());
                    Assert.IsFalse(previousGuid == thisGuid);
                }
                previousGuid = thisGuid;
            }
        }

        [TestMethod]
        public void TestBytesToGuid()
        {
            LogStart("TestBytesToGuid");

            Guid result = (new byte[]{255,255}).ToGuid();
            Assert.IsTrue(result == new Guid(new byte[16]{ 255,255,0,0,0,0,0,0,0,0,0,0,0,0,0,0}));
            result = (new byte[]{0,55,67,89}).ToGuid();
            Assert.IsTrue(result == new Guid(new byte[16]{ 0,55,67,89,0,0,0,0,0,0,0,0,0,0,0,0}));
            result = (new byte[20] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }).ToGuid();
            Assert.IsTrue(result == new Guid(new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}));
        }

        [TestMethod]
        public void TestHumanReadableBytes()
        {
            long bytes = 74567;
            Assert.IsTrue(bytes.ToByteUnits(1) == "72.8KiB");
            bytes = 908070605;
            Assert.IsTrue(bytes.ToByteUnits(1) == "866.0MiB");
            bytes = 1234567890;
            Assert.IsTrue(bytes.ToByteUnits(1) == "1.1GiB");
            Assert.IsTrue(bytes.ToByteUnits(2) == "1.15GiB");
            Assert.IsTrue(bytes.ToByteUnits(3) == "1.150GiB");
            Assert.IsTrue(bytes.ToByteUnits(4) == "1.1498GiB");
            bytes = 898;
            Assert.IsTrue(bytes.ToByteUnits(1) == "898B");
        }

        [TestMethod]
        public void TestHumanReadableTimeSpan()
        {
            var time = TimeSpan.FromSeconds(5);
            Assert.IsTrue(time.ToSimplePhrase(0) == "5 seconds");
            Assert.IsTrue(time.ToSimplePhrase(1) == "5.0 seconds");
            time = TimeSpan.FromDays(1.3456);
            Assert.IsTrue(time.ToSimplePhrase(0) == "1 day");
            Assert.IsTrue(time.ToSimplePhrase(1) == "1.3 days");
            time = TimeSpan.FromDays(900);
            Assert.IsTrue(time.ToSimplePhrase(0) == "2 years");
            //Assert.IsTrue(time.ToSimplePhrase(1) = "1.3 days")
        }


        [TestMethod]
        public void TestLSBArray()
        {
            int data = 0xFFAB;
            var result = ByteExtensions.GetLSBArrayFromValue(data, 2);
            Assert.IsTrue(result[0] == 0xAB);
            Assert.IsTrue(result[1] == 0xFF);
            Assert.IsTrue(ByteExtensions.GetValueFromLSBArray(result) == data);
            data = 0xABCDEF;
            result = ByteExtensions.GetLSBArrayFromValue(data, 2);
            Assert.IsTrue(result[0] == 0xEF);
            Assert.IsTrue(result[1] == 0xCD);
            Assert.IsTrue(ByteExtensions.GetValueFromLSBArray(result) == 0xCDEF);
            result = ByteExtensions.GetLSBArrayFromValue(data);
            Assert.IsTrue(result[0] == 0xEF);
            Assert.IsTrue(result[1] == 0xCD);
            Assert.IsTrue(result[2] == 0xAB);
            Assert.IsTrue(ByteExtensions.GetValueFromLSBArray(result) == data);
            Assert.IsTrue(ByteExtensions.GetValueFromLSBArray(result, 1, 2) == 0xABCD);
            Assert.IsTrue(ByteExtensions.GetValueFromLSBArray(result, 2, 2) == 0xAB);
        }

        [TestMethod]
        public void TestReverseBits()
        {
            byte data = 0xF0;
            byte result = data.ReverseBits();
            Assert.IsTrue(result == 0x0F);

            data = 0x81;
            result = data.ReverseBits();
            Assert.IsTrue(result == 0x81);

            for (int i = 0; i < 1000000; i++)
            {
                data = (byte)(i % 256);
                Assert.IsTrue(data.ReverseBits() == ByteExtensions.BitReverseTable[data]);
            }
        }

    }
}
