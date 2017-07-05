
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using MonolithicExtensions.General.Logging;
using System.Linq;

namespace MonolithicExtensions.Windows
{
    public static class RegistryServices
    {
        private static log4net.ILog Logger { get; } = log4net.LogManager.GetLogger(typeof(RegistryServices));

        public static Dictionary<Guid, ComRegistryData> GetComClsidMapping(bool includeUnknowns = false)
        {
            Logger.Trace("Starting GetComClsidMapping");

            var mapping = GetComClsidMappingGeneric("CLSID", includeUnknowns);
            var wow64Mapping = GetComClsidMappingGeneric("WOW6432Node\\CLSID");

            //Make regular mapping override wow64 mappings
            foreach (var key in wow64Mapping.Keys)
            {
                if (!mapping.ContainsKey(key))
                {
                    mapping.Add(key, wow64Mapping[key]);
                }
            }

            return mapping;
        }

        private static Dictionary<Guid, ComRegistryData> GetComClsidMappingGeneric(string startKey, bool includeUnknowns = false)
        {
            var mapping = new Dictionary<Guid, ComRegistryData>();
            var baseKey = Registry.ClassesRoot.OpenSubKey(startKey);
            var guidMatch = new Regex("{([0-9A-F\\-]+)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (baseKey == null)
            {
                Logger.Warn("The registry doesn't contain the key " + startKey + " in CLASSES_ROOT");
                return mapping;
            }

            foreach (var subKeyName in baseKey.GetSubKeyNames())
            {
                Guid clsid = new Guid();
                var keyMatch = guidMatch.Match(subKeyName);

                //Simply do not get keys for COMS whose CLSID is not a real guid
                if (!keyMatch.Success)
                    continue;

                if (!Guid.TryParse(keyMatch.Groups[1].Value, out clsid))
                {
                    Logger.Warn("Could not parse CLSID from registry: " + subKeyName);
                    continue;
                }

                //Now get the good stuff!
                var clsidKey = baseKey.OpenSubKey(subKeyName);
                var inproc = clsidKey.OpenSubKey("InprocServer32");

                //Stop if there's no inproc
                if (inproc == null)
                    continue;

                try
                {
                    //Once again, even though we found the inproc key, there may not be a value, so just skip it if so.
                    var inprocValue = Convert.ToString(inproc.GetValue("", ""));
                    if (string.IsNullOrWhiteSpace(inprocValue))
                        continue;

                    var data = new ComRegistryData(inprocValue);

                    //Unless the user wants us to include unknowns, do NOT include the data if it's just a combase.dll
                    if (includeUnknowns || (!data.IsComBase & Path.IsPathRooted(data.ExpandedPath)))
                    {
                        mapping.Add(clsid, data);
                    }

                }
                catch (ArgumentException ex)
                {
                    //Ignore argument exceptions
                }
                catch (Exception ex)
                {
                    Logger.Warn("Exception while parsing InprocServer32 registry value: " + ex.ToString());
                }

            }

            return mapping;
        }

        /// <summary>
        /// Retrieve all the paths registered for the given filename using the given dictionary of registry data.
        /// </summary>
        /// <param name="Filename">JUST the filename of the COM you want to lookup (no path)</param>
        /// <param name="RegistryData">Can be obtained by calling GetComClsidMapping()</param>
        /// <returns>All the paths associated with that file (duplicates removed)</returns>
        public static List<string> GetRegisteredPathsForComFile(string Filename, Dictionary<Guid, ComRegistryData> RegistryData)
        {
            return RegistryData.Values.Where(x => x.BaseFilename == Filename).Select(x => x.ExpandedPath).Distinct().ToList();
        }

        public static RegFileData ParseRegFileString(string Contents)
        {
            var lines = Contents.Split("\n".ToCharArray(), StringSplitOptions.None).Select(x => x.Trim()).ToList();
            var data = new RegFileData();
            var leftBracket = Regex.Escape("[");
            var rightBracket = Regex.Escape("]");
            var keyMatcher = new Regex("^\\s*" + leftBracket + "([^" + rightBracket + "]+)" + rightBracket + "\\s*$");

            var currentKey = "";

            foreach (var line in lines)
            {
                var match = keyMatcher.Match(line);

                //Oh, we found a key. All future subkey/value pairs will go to this key.
                if (match.Success)
                {
                    currentKey = match.Groups[1].Value;
                    if (!data.KeyValuePairs.ContainsKey(currentKey))
                    {
                        data.KeyValuePairs.Add(currentKey, new Dictionary<string, RegFileValue>());
                    }
                }
            }

            return data;
        }

        public static RegFileData ParseRegFile(string FilePath)
        {
            return ParseRegFileString(File.ReadAllText(FilePath));
        }

    }

    public class RegFileData
    {
        public Dictionary<string, Dictionary<string, RegFileValue>> KeyValuePairs { get; set; } = new Dictionary<string, Dictionary<string, RegFileValue>>();
    }

    public class RegFileValue
    {
        public object RawValue { get; set; } = null;
        public RegFileType GivenType { get; set; } = 0;  //RegFileType.hex2;
    }

    public enum RegFileType
    {
        hex2,
        hex7
    }

    public class ComRegistryData
    {
        /// <summary>
        /// The raw value obtained from the InprocServer32 field.
        /// </summary>
        /// <returns></returns>
        public string RawInprocServer32 { get; }

        /// <summary>
        /// The expanded path for this COM DLL
        /// </summary>
        /// <returns></returns>
        public string ExpandedPath { get; }

        /// <summary>
        /// The filename for this COM DLL
        /// </summary>
        /// <returns></returns>
        public string BaseFilename { get; }

        /// <summary>
        /// Whether or not the COM data found in the registry just points to combase.dll (which some RegistryService functions don't handle)
        /// </summary>
        /// <returns></returns>
        public bool IsComBase
        {
            get { return RawInprocServer32 == "combase.dll"; }
        }

        public ComRegistryData(string InprocServer32)
        {
            RawInprocServer32 = InprocServer32;
            ExpandedPath = Environment.ExpandEnvironmentVariables(RawInprocServer32);
            BaseFilename = Path.GetFileName(ExpandedPath);

        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}