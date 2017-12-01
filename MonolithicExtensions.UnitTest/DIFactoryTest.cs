using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Portable;
using MonolithicExtensions.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class DIFactoryTest : UnitTestBase
    {

        [TestMethod()]
        public void BasicDIFactoryCreate()
        {
            DIFactory factory = new DIFactory();

            factory.CreationMapping.Add(typeof(MyCreatingThing), (DIFactory f) => { return new MyCreatingThing { Nuts = 88 }; });

            Assert.IsTrue(factory.Create<MyCreatingThing>().Nuts == 88);

            factory.CreationMapping.Add(typeof(IThingy), (DIFactory f) => { return new MyThingyImplemented(); });

            Assert.IsTrue(factory.Create<IThingy>().GetAValue() == 123);
            MyAssert.ThrowsException(() => factory.Create<DIFactory>());

            MyCreatingThing specialSetting = new MyCreatingThing { Nuts = 678 };
            factory.SetSettingByType(specialSetting);

            Assert.IsTrue(factory.GetSettingByType<MyCreatingThing>().Nuts == 678);
        }

        [TestMethod()]
        public void TestDIFactoryMerge()
        {
            DIFactory factory = new DIFactory();

            factory.CreationMapping.Add(typeof(MyCreatingThing), (DIFactory f) => { return new MyCreatingThing { Nuts = f.GetSetting<ThingSettings, int>(ThingSettings.One) }; });

            factory.ReleaseMapping.Add(typeof(MyCreatingThing), (DIFactory f, object o) => { Logger.Info("releasing a MyCreatingThing"); });

            factory.SetSetting(ThingSettings.One, 999);

            DIFactory factory2 = new DIFactory();

            factory2.CreationMapping.Add(typeof(IThingy), (DIFactory f) => { return new MyThingyImplemented(); });

            factory2.ReleaseMapping.Add(typeof(IThingy), (DIFactory f, object o) => { Logger.Info("releasing an IThingy"); });

            factory2.SetSetting(ThingSettings.Two, "A lot of whatever");

            var newFactory = DIFactory.Merge(factory, factory2);

            Assert.IsTrue(newFactory.Create<MyCreatingThing>().Nuts == 999);
            Assert.IsTrue(newFactory.GetSetting<ThingSettings, string>(ThingSettings.Two) == "A lot of whatever");
            MyAssert.ThrowsException(() => DIFactory.Merge(newFactory, factory));
            MyAssert.ThrowsException(() => DIFactory.Merge(newFactory, factory2));
        }

        protected class MyCreatingThing
        {
            public int Nuts { get; set; }
        }

        protected interface IThingy
        {
            int GetAValue();
        }

        protected class MyThingyImplemented : IThingy
        {
            public int GetAValue()
            {
                return 123;
            }
        }

        protected enum ThingSettings
        {
            One,
            Two,
            Three
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
