using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.General;
using MonolithicExtensions.Windows;
using MonolithicExtensions.Portable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class SerializationTest : UnitTestBase
    {
        const string TestFile = "test.json";

        [TestMethod()]
        public void SimpleSerializationTest()
        {
            int regularInteger = 10;

            MySerialize.SaveObject(TestFile, regularInteger);
            Assert.IsTrue(MySerialize.LoadObject<int>(TestFile) == regularInteger);

            string regularString = "something Or Whatever '\\/'}{";
            MySerialize.SaveObject(TestFile, regularString);
            Assert.IsTrue(MySerialize.LoadObject<string>(TestFile) == regularString);

            MySerialize.SaveObject(TestFile, SimpleList);
            Assert.IsTrue(MySerialize.LoadObject<List<int>>(TestFile).SequenceEqual(SimpleList));

            MySerialize.SaveObject(TestFile, SimpleDictionary);
            Assert.IsTrue(MySerialize.LoadObject<Dictionary<int, string>>(TestFile).ToList().SequenceEqual(SimpleDictionary.ToList()));
        }

        [TestMethod()]
        public void InheritanceSerializationTest()
        {
            MySerialize.SaveObject(TestFile, SimpleInheritedClass, true);
            InheritedContainer newContainer = MySerialize.LoadObject<InheritedContainer>(TestFile);
            Assert.IsTrue(newContainer.Equals(SimpleInheritedClass));
        }

        [TestMethod()]
        public void ComplexReferenceSerializationTest()
        {
            MySerialize.SaveObject(TestFile, ComplexClassList, true);
            var newList = MySerialize.LoadObject<List<ComplexContainer>>(TestFile);
            Assert.IsTrue(newList.SequenceEqual(ComplexClassList));
        }

        [TestMethod()]
        public void ComplexClassSerializationTest()
        {
            MySerialize.SaveObject(TestFile, SimpleClass, true);
            Assert.IsFalse(SimpleClass.Equals(new ComplexContainer()));
            var regularObject = MySerialize.LoadObject<ComplexContainer>(TestFile);
            Assert.IsTrue(regularObject.Equals(SimpleClass));
        }

        [TestMethod()]
        public void SerializeClassWithLoggerTest()
        {
            RestartManagerExtendedSession testObject = new RestartManagerExtendedSession();
            testObject.ManualRestartProcesses.Add("yoyoyo");
            Assert.IsTrue(testObject.SessionKey != (new RestartManagerExtendedSession()).SessionKey);
            MySerialize.SaveObject(TestFile, testObject);
            System.Threading.Thread.Sleep(10);
            var nextObject = MySerialize.LoadObject<RestartManagerExtendedSession>(TestFile);
            Assert.IsTrue(nextObject.SessionKey == testObject.SessionKey);
            Assert.IsTrue(nextObject.ManualRestartProcesses.IsEquivalentTo(testObject.ManualRestartProcesses));
        }

        [TestMethod()]
        public void JsonTransporterTest()
        {
            var transporter = JsonTransporter.Create<ComplexContainer>(SimpleClass);
            Assert.IsFalse(SimpleClass.Equals(new ComplexContainer()));
            Assert.IsTrue(transporter.GetObject().Equals(SimpleClass));
        }

        private class Whatever
        {
            public Guid ID = Guid.NewGuid();
            public int Count = 0;
            public string Yeah = "Yeah";
        }

        [TestMethod]
        public void SerializeWithMissingData()
        {
            Whatever result = null;
            result = MySerialize.JsonParse<Whatever>(@"{""ID"":""9a5e2128-f535-45c2-9d03-e8a8137633f4""}");
            Assert.IsTrue(result.ID == new Guid("9a5e2128-f535-45c2-9d03-e8a8137633f4"));
            Assert.IsTrue(result.Yeah == "Yeah");
            result = MySerialize.JsonParse<Whatever>(@"{""Count"":66}");
            Assert.IsFalse(result.ID == new Guid("9a5e2128-f535-45c2-9d03-e8a8137633f4"));
            Assert.IsTrue(result.Count == 66);
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
