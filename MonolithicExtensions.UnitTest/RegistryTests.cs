
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonolithicExtensions.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MonolithicExtensions.Portable;

[TestClass()]
public class RegistryTest : UnitTestBase
{
    public const string TestRegFile = "ComputerName.reg";
    public List<string> ExpectedRegKeys { get; } = new List<string>()
    {
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\DefaultIcon",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\InProcServer32",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\find",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\find\\command",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\find\\ddeexec",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\find\\ddeexec\\application",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\find\\ddeexec\\topic",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\Manage",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\shell\\Manage\\command",
        "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\CLSID\\{20D04FE0-3AEA-1069-A2D8-08002B30309D}\\ShellFolder"
    };

    [TestMethod()]
    public void TestCLSIDRegistryPull()
    {
        var comMapping = RegistryServices.GetComClsidMapping();

        //All windows systems should have more than 100 COMs registered... right???
        Assert.IsTrue(comMapping.Keys.Count() > 100);
    }

    [TestMethod()]
    public void TestCLSIDRegistryPathfind()
    {
        var comMapping = RegistryServices.GetComClsidMapping();

        var paths = RegistryServices.GetRegisteredPathsForComFile("dao360.dll", comMapping);
        Assert.IsTrue(paths.Count == 1);
        Assert.IsTrue(paths.First() == "C:\\Program Files (x86)\\Common Files\\Microsoft Shared\\DAO\\dao360.dll");
    }

    [TestMethod()]
    public void TestRegFileKeyRetrieval()
    {
        var data = RegistryServices.ParseRegFile(TestRegFile);
        Assert.IsTrue(data.KeyValuePairs.Keys.ToList().IsEquivalentTo(ExpectedRegKeys));
    }

}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
