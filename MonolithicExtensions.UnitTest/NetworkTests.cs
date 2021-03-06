﻿
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Portable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MonolithicExtensions.Windows;
using System.IO;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class NetworkTest : UnitTestBase
    {

        [TestMethod()]
        public void CRC16Test()
        {
            Func<IList<byte>, byte[]> CRCFunction = HashServices.CRC16_XMODEM;

            Action<string, int> testCRC16 = (string data, int expected) =>
            {
                byte[] crc = CRCFunction(Encoding.ASCII.GetBytes(data));
                ushort crcValue = BitConverter.ToUInt16(crc, 0);
                Assert.IsTrue(crcValue == expected);
            };

            CRCFunction = HashServices.CRC16_CCITTFalse;
            testCRC16("A", 0x9479);
            testCRC16("0", 0xfacf);
            testCRC16("123456789", 0xe5cc);
            CRCFunction = HashServices.CRC16_XMODEM;
            testCRC16("A", 0x58e5);
            testCRC16("0", 0x3653);
            testCRC16("123456789", 0x31c3);
            CRCFunction = HashServices.CRC16_KERMIT;
            testCRC16("A", 0x8d53);
            testCRC16("0", 0x8331);
            testCRC16("123456789", 0x8921);
            testCRC16("This is just a test string that should be long enough and weird enough to catch ERRORS!", 0x9248);

        }

        [TestMethod()]
        public void CRC32Test()
        {
            LogStart("CRC32Test");

            Action<uint, uint> testReverse = (uint original, uint expected) => { Assert.IsTrue(HashServices.ReverseBits(original) == expected); };

            Action<string, uint> testCRC32 = (string data, uint expected) =>
            {
                byte[] crc = HashServices.CRC32(Encoding.ASCII.GetBytes(data));
                uint crcValue = BitConverter.ToUInt32(crc, 0);
                Assert.IsTrue(crcValue == expected);
            };

            testReverse(0x1, 0x80000000u);
            testReverse(0x2, 0x40000000u);
            testReverse(0x4, 0x20000000u);
            testReverse(0x8, 0x10000000u);
            testReverse(0xd2d15a39u, 0x9c5a8b4bu);
            Assert.IsFalse(HashServices.ReverseBits(0xd2d15a39u) == 0x9c5b8b4bu);

            testCRC32("123456789", 0xcbf43926u);
            testCRC32("A", 0xd3d99e8bu);
            testCRC32("0", 0xf4dbdf21u);
            testCRC32("This is just a test string that should be long enough and weird enough to catch ERRORS!", 0x6684ac42u);

            DateTime start;
            uint result;

            //And now the big one: will a huge file still produce the right crc32 using the stream method?
            using (FileStream f = new FileStream("MediaCreationTool.exe", FileMode.Open))
            {
                start = DateTime.Now;
                result = BitConverter.ToUInt32(HashServices.CRC32(f), 0);
                Logger.Info($"CRC32 of 17.7mb file took {(DateTime.Now - start).ToSimplePhrase()}");
                Assert.IsTrue(result == 0x5a2DD3B4);
            }

            //Oh and might as well test time for normal... crc
            //start = DateTime.Now;
            //result = BitConverter.ToUInt32(NetworkServices.CRC32(File.ReadAllBytes("MediaCreationTool.exe")), 0);
            //Logger.Info($"CRC32 of 17.7mb file took {(DateTime.Now - start).ToSimplePhrase()}");
            //Assert.IsTrue(result == 0x5a2DD3B4);
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
