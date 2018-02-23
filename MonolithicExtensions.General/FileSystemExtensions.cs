
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonolithicExtensions.General
{
    public static class FileServices
    {
        public class SplitFileSettings
        {
            public char[] Delimiters { get; set; } = "\t".ToCharArray();
            public bool RemoveEmptyEntries { get; set; } = true;
            public bool RemoveEmptyLines { get; set; } = true;
            public bool TrimEntries { get; set; } = true;
            public TimeSpan FileReadTimeout { get; set; } = TimeSpan.FromSeconds(5);
        }

        public static List<List<string>> SplitDelimitedFileByLines(string filePath, SplitFileSettings settings)
        {
            List<string> lines = new List<string>();
            ThreadingServices.WaitOnAction(() => lines = File.ReadAllLines(filePath).ToList(), settings.FileReadTimeout);
            List<List<string>> results = new List<List<string>>();
            foreach (var line in lines)
            {
                var entries = line.Split(settings.Delimiters).ToList();
                if (settings.TrimEntries)
                    entries = entries.Select(x => x.Trim()).ToList();
                if (settings.RemoveEmptyEntries)
                    entries = entries.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (settings.RemoveEmptyLines & entries.Count < 1)
                    continue;
                results.Add(entries);
            }
            return results;
        }

        public static Version GetFileVersionOfDll(string dllName)
        {
            //Process.GetCurrentProcess().Modules
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyPath = assembly.Location;
                //ProcessModule.FileName
                if (Path.GetFileName(assemblyPath).ToUpper().Contains(dllName.ToUpper()))
                {
                    return new Version(FileVersionInfo.GetVersionInfo(assemblyPath).FileVersion);
                }
            }
            throw new InvalidOperationException("There is no DLL that contains the name " + dllName);
        }

        public static Version GetOwnFileVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule.FileName).FileVersion);
        }

        /// <summary>
        /// Force a file to be deleted, even if it is read-only. Retries for the specified timeout period
        /// (default 3 seconds) if it fails.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="timeout"></param>
        public static void ForceDelete(string filename, TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(3);
            TimeSpan realTimeout = (TimeSpan)timeout;
            ThreadingServices.WaitOnAction(() => File.SetAttributes(filename, FileAttributes.Normal), realTimeout);
            ThreadingServices.WaitOnAction(() => File.Delete(filename), realTimeout);
        }

        /// <summary>
        /// Force a file to be copied, even if the destination exists and is read-only. Retries for the specified
        /// timeout period (default 3 seconds) if it fails
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="timeout"></param>
        public static void ForceCopy(string source, string destination, TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(3);
            TimeSpan realTimeout = (TimeSpan)timeout;
            if (File.Exists(destination))
                ForceDelete(destination, realTimeout);
            ThreadingServices.WaitOnAction(() => File.Copy(source, destination, true), realTimeout);
        }
    }

    public static class DirectoryExtensions
    {
        public static string GetCurrentExecutableDirectory()
        {
            var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            return Path.GetDirectoryName(Uri.UnescapeDataString((new Uri(codeBase)).AbsolutePath));
        }

        public static string GetAbsolutePathBasedOnExecutableDirectory(string filepath)
        {
            if (Path.IsPathRooted(filepath))
                return filepath;
            return Path.Combine(GetCurrentExecutableDirectory(), filepath);
        }

        /// <summary>
        /// Force a directory to be deleted, even if it contains read-only files. Retries for the specified
        /// timeout period (default 3 seconds) if it fails. 
        /// </summary>
        /// <remarks>This method forces every file to have normal file permissions, and DOES NOT
        /// put the permissions back to normal if it fails. If this is unnacceptable, you may want to
        /// write a custom method using FileServices.ForceDelete</remarks>
        /// <param name="directoryPath"></param>
        /// <param name="timeout"></param>
        public static void ForceDelete(string directoryPath, TimeSpan? timeout = null)
        {
            if (timeout == null)
                timeout = TimeSpan.FromSeconds(3);
            TimeSpan realTimeout = (TimeSpan)timeout;
            DateTime start = DateTime.Now;
            foreach (var filename in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                if (File.GetAttributes(filename) != FileAttributes.Normal)
                {
                    ThreadingServices.WaitOnAction(() => File.SetAttributes(filename, FileAttributes.Normal), realTimeout - (DateTime.Now - start));
                }
            }
            ThreadingServices.WaitOnAction(() => Directory.Delete(directoryPath, true), realTimeout - (DateTime.Now - start));
            ThreadingServices.WaitOnAction(() => { if (Directory.Exists(directoryPath)) throw new Exception("Directory still exists"); }, realTimeout - (DateTime.Now - start));
        }

    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
