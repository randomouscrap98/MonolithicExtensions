using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.General;
using MonolithicExtensions.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class FileTest
    {
        public const string TempFile = "tempt.txt";

        [TestMethod()]
        public void TestForceCopyDelete()
        {
            string filename = "testfile_forced.txt";
            string filetext = "This is the file text. It is unique! " + Guid.NewGuid().ToString();

            File.WriteAllText(filename, filetext);
            File.SetAttributes(filename, FileAttributes.ReadOnly);
            MyAssert.ThrowsException(() => File.Delete(filename));
            Assert.IsTrue(File.ReadAllText(filename) == filetext);
            FileServices.ForceDelete(filename);
            Assert.IsFalse(File.Exists(filename));

            string copyname = "copy" + filename;
            string copytext = "It's a copy " + Guid.NewGuid().ToString();
            File.WriteAllText(filename, filetext);
            File.WriteAllText(copyname, copytext);
            File.SetAttributes(filename, FileAttributes.ReadOnly);
            MyAssert.ThrowsException(() => File.Copy(copyname, filename));
            Assert.IsTrue(File.ReadAllText(filename) == filetext);
            Assert.IsTrue(File.ReadAllText(copyname) == copytext);
            FileServices.ForceCopy(copyname, filename);
            Assert.IsTrue(File.ReadAllText(filename) == copytext);
            Assert.IsTrue(File.ReadAllText(copyname) == copytext);
        }

        [TestMethod()]
        public void TestSplitFile()
        {
            string contents = string.Format("one{0}two{0}three{1}four{0}{0}five and{0}six{1}", "\t", Environment.NewLine);
            File.WriteAllText(TempFile, contents);
            var parsed = FileServices.SplitDelimitedFileByLines(TempFile, new FileServices.SplitFileSettings());
            Assert.IsTrue(parsed[0][0] == "one");
            Assert.IsTrue(parsed[0][1] == "two");
            Assert.IsTrue(parsed[0][2] == "three");
            Assert.IsTrue(parsed[1][0] == "four");
            Assert.IsTrue(parsed[1][1] == "five and");
            Assert.IsTrue(parsed[1][2] == "six");
            Assert.IsTrue(parsed.Count == 2);
            Assert.IsTrue(parsed[0].Count == 3);
            Assert.IsTrue(parsed[1].Count == 3);
        }

        [TestMethod()]
        public void TestFileVersion()
        {
            Version expectedVersion = new Version("9.8.7.6");
            var actualVersion = FileServices.GetFileVersionOfDll("MonolithicExtensions.UnitTest");
            Assert.IsTrue(actualVersion.Equals(expectedVersion));
            MyAssert.ThrowsException(() => FileServices.GetFileVersionOfDll("SomethingThatDoesntExist"));
            actualVersion = FileServices.GetOwnFileVersion();
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
