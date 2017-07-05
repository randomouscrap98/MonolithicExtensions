
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Runtime.CompilerServices;

namespace MonolithicExtensions.Windows
{
    public static class FontServices
    {
        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern int AddFontResource(string lpsqFileName);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern int CreateScalableFontResource(uint fdwHidden, string lpszFontRes, string lpszFontFile, string lpszCurrentPath);

        /// <summary>
        /// Permanently (?) register the given font file. Will overwrite any existing font files.
        /// </summary>
        /// <param name="fontFilePath"></param>
        /// <returns>False if the font already exists and does not need to be registered.</returns>
        public static bool RegisterFont(string fontFilePath)
        {

            string fontFileName = Path.GetFileName(fontFilePath);
            var fontDestination = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Fonts), fontFileName);

            //Only attempt registration if the font doesn't already exist
            if (!File.Exists(fontDestination))
            {
                File.Copy(fontFilePath, fontDestination);

                PrivateFontCollection fontCollection = new PrivateFontCollection();
                fontCollection.AddFontFile(fontDestination);
                var actualFontName = fontCollection.Families[0].Name;

                AddFontResource(fontDestination);
                Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", actualFontName, fontFileName, RegistryValueKind.String);

                return true;
            }

            return false;

        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
