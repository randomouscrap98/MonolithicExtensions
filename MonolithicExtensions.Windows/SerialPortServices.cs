using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Management;

namespace MonolithicExtensions.Windows
{
    public static class SerialPortServices
    {
        public static List<SerialPortInfo> GetAllSerialPortInfo()
        {
            List<SerialPortInfo> results = new List<SerialPortInfo>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_SerialPort");

            foreach (ManagementObject queryObject in searcher.Get())
            {
                results.Add(new SerialPortInfo(queryObject));
            }

            return results;
        }
    }

    public class SerialPortInfo
    {
        public string Name { get; }
        public string Description { get; } = "";
        public string DeviceID { get; } = "";
        public string Caption { get; } = "";
        public string Manufacturer { get; } = "";

        public SerialPortInfo(ManagementObject qo)
        {
            Name = Convert.ToString(qo["Name"]);

            try { Description = Convert.ToString(qo["Description"]); }
            catch { /*do nothing*/ }

            try { DeviceID = Convert.ToString(qo["DeviceID"]); }
            catch { /*do nothing*/ }

            try { Caption = Convert.ToString(qo["Caption"]); }
            catch { /*do nothing*/ }

            try { Manufacturer = Convert.ToString(qo["Manufacturer"]); }
            catch { /*do nothing*/ }
        }

        //Public Sub New(name As String, Optional description As String = "", Optional DeviceID As String = "",
        //               Optional Caption As String = "", Optional Manufacturer As String = "")
        //    Me.Name = name
        //    Me.Description = description
        //    Me.Caption = Caption
        //    Me.DeviceID = DeviceID
        //    Me.Manufacturer = Manufacturer
        //End Sub
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
