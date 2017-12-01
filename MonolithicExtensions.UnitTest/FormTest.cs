
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace MonolithicExtensions.UnitTest
{
    [TestClass()]
    public class FormsTest
    {
        [TestMethod()]
        public void TestFindControlsByName()
        {
            Form form = new Form();
            List<string> names = new List<string> {
                "bacon",
                "eggs",
                "rice",
                "noodles",
                "bacon",
                "broccolli"
            };

            var i = 0;

            foreach (var name in names)
            {
                form.Controls.Add(new Control
                {
                    Name = name,
                    TabIndex = i
                });
                i += 1;
            }

            Assert.IsTrue(form.FindControlsByName("bacon").Count == 2);
            Assert.IsTrue(form.FindControlsByName("eggs").FirstOrDefault().TabIndex == 1);
            Assert.IsTrue(form.FindControlsByName("nothing").Count == 0);
        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
