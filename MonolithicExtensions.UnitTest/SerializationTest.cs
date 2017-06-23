﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var nextObject = MySerialize.LoadObject<RestartManagerExtendedSession>(TestFile);
            Assert.IsTrue(nextObject.SessionKey == testObject.SessionKey);
            //regularObject.Equals(SimpleClass))
            Assert.IsTrue(nextObject.ManualRestartProcesses.IsEquivalentTo(testObject.ManualRestartProcesses));
            //regularObject.Equals(SimpleClass))
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
