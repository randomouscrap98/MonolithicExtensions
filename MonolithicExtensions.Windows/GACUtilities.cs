
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace MonolithicExtensions.Windows
{
    //Kindly given by http://stackoverflow.com/a/2611435/1066474 with a few modifications
    public static class GacUtilities
    {
        public static void InstallAssembly(string path, bool forceRefresh)
        {
            IAssemblyCache iac = null;
            CreateAssemblyCache(ref iac, 0);
            try
            {
                uint flags = 1;
                if (forceRefresh)
                    flags = 2;
                int hr = iac.InstallAssembly(flags, path, IntPtr.Zero);
                if ((hr < 0))
                    Marshal.ThrowExceptionForHR(hr);
            }
            finally
            {
                Marshal.FinalReleaseComObject(iac);
            }
        }

        public static void UninstallAssembly(string displayName)
        {
            IAssemblyCache iac = null;
            CreateAssemblyCache(ref iac, 0);
            try
            {
                uint whatHappened = 0;
                int hr = iac.UninstallAssembly(0, displayName, IntPtr.Zero, ref whatHappened);
                if ((hr < 0))
                    Marshal.ThrowExceptionForHR(hr);
                switch ((whatHappened))
                {
                    case 1: return;
                    case 2: throw new InvalidOperationException("Assembly still in use");
                    case 3: throw new InvalidOperationException("Already already uninstalled");
                    case 5: throw new InvalidOperationException("Assembly still has install references");
                    case 6: throw new System.IO.FileNotFoundException(); //Not actually raised?
                    default: throw new InvalidOperationException("Unknown error: " + whatHappened);
                }
            }
            finally
            {
                Marshal.FinalReleaseComObject(iac);
            }
        }


        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        internal interface IAssemblyCache
        {
            [PreserveSig()]
            int UninstallAssembly(uint fags, [MarshalAs(UnmanagedType.LPWStr)]
string assemblyName, IntPtr pvReserved, ref uint pulDisposition);
            [PreserveSig()]
            int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)]
string pszAssemblyName, IntPtr pAsmInfo);
            [PreserveSig()]
            int CreateAssemblyCacheItem();
            [PreserveSig()]
            int CreateAssemblyScavenger(ref object ppAsmScavenger);
            [PreserveSig()]
            int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)]
string pszManifestFilePath, IntPtr pvReserved);
        }

        // NOTE use "clr.dll" in .NET 4+
        [DllImport("clr.dll", PreserveSig = false)]
        static internal extern void CreateAssemblyCache(ref IAssemblyCache ppAsmCache, int reserved);

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
