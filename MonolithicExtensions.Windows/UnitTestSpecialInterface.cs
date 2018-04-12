using MonolithicExtensions.General;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceModel;

namespace MonolithicExtensions.Windows.UnitTestSpecial
{
    /// <summary>
    /// The functions exposed by the test service which allow them to be performed as the system account
    /// </summary>
    [ServiceContract()]
    public interface ISpecialTestContract
    {
        [OperationContract()]
        CapturedExceptionResultTransporter<bool> GetCurrentUserToken();

        [OperationContract()]
        CapturedExceptionResultTransporter<bool> GetExplorerUserToken();

        [OperationContract()]
        CapturedExceptionResultTransporter<bool> StartProcessAsCurrentUser(string expectedUser);
    }

    public static class TestHelpers
    {
        public const string ExtensionsTestAddress = "net.pipe://localhost/";
        public const string ExtensionsServiceName = "monolithicextensionstest";

        /// <summary>
        /// The name of the installed service for unit testing.
        /// </summary>
        /// <remarks>CHANGE THIS FIELD IF YOU CHANGE THE NAME OF THE SERVICE!</remarks>
        public const string TestService = "MonolithicExtensions.TestService";
    }

    public class SpecialTestService : ISpecialTestContract
    {
        protected Portable.Logging.ILogger Logger { get; } 

        public SpecialTestService()
        {
            Logger = Portable.Logging.LogServices.CreateLoggerFromDefault(this.GetType());
        }

        public string GetProcessOwner(int processId)
        {
            var query = "Select * From Win32_Process Where ProcessID = " + processId;
            var searcher = new ManagementObjectSearcher(query);
            var processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                var argList = new string[] {
                    string.Empty,
                    string.Empty
                };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                    return argList[1] + "\\" + argList[0];
            }

            throw new InvalidOperationException("Could not retrieve process owner!");
        }

        public CapturedExceptionResultTransporter<bool> GetCurrentUserToken()
        {
            try
            {
                IntPtr token = new IntPtr();
                return new CapturedExceptionResultTransporter<bool>(WindowsAccountServices.GetSessionUserToken(ref token));
            }
            catch (Exception ex)
            {
                return new CapturedExceptionResultTransporter<bool>(ex);
            }
        }

        public CapturedExceptionResultTransporter<bool> GetExplorerUserToken()
        {
            try
            {
                var explorerProcesses = Process.GetProcessesByName("explorer");
                Logger.Debug("Explorer process count: " + explorerProcesses.Count());
                foreach (Process explorer in explorerProcesses)
                {
                    IntPtr userToken = IntPtr.Zero;
                    if (!WindowsAccountServices.GetUserTokenFromProcess(explorer.Id, ref userToken) || userToken == IntPtr.Zero)
                    {
                        Logger.Error("Could not retrieve user token from explorer process " + explorer.Id + ": userToken=" + userToken.ToInt32() + ", error code: " + Marshal.GetLastWin32Error());
                        throw new Exception("Could not retrieve user token from explorer process " + explorer.Id);
                    }
                    else
                    {
                        Logger.Info("Sucessfully retrieved user token for Explorer process " + explorer.Id);
                    }
                    WindowsAccountServices.CloseHandle(userToken);
                }
                return new CapturedExceptionResultTransporter<bool>(true);
            }
            catch (Exception ex)
            {
                return new CapturedExceptionResultTransporter<bool>(ex);
            }
        }

        public CapturedExceptionResultTransporter<bool> StartProcessAsCurrentUser(string expectedUser)
        {
            try
            {
                var currentnotepadProcesses = Process.GetProcessesByName("notepad");
                Logger.Debug("Before running, there were " + currentnotepadProcesses.Count() + " notepad processes");
                if (!WindowsAccountServices.StartProcessAsCurrentUser("C:\\WINDOWS\\system32\\notepad.exe"))
                    throw new Exception("Could not start notepad as current user!");
                var notepadProcesses = Process.GetProcessesByName("notepad");
                Logger.Debug("Notepad process count: " + notepadProcesses.Count());
                var newNotepadProcesses = notepadProcesses.Where(x => !currentnotepadProcesses.Any(y => y.Id == x.Id));
                Logger.Debug("Notepad processes that we appeared to create: " + newNotepadProcesses.Count());
                if (newNotepadProcesses.Count() == 0)
                    throw new Exception("The notepad process doesn't appear to have started!");
                int notepadProcessesByUser = 0;
                foreach (Process notepadProcess in newNotepadProcesses)
                {
                    if (GetProcessOwner(notepadProcess.Id) == expectedUser)
                        notepadProcessesByUser += 1;
                    notepadProcess.Kill();
                }
                if (notepadProcessesByUser < 1)
                    throw new Exception("The notepad process did not have the expected user!");
                return new CapturedExceptionResultTransporter<bool>(true);
            }
            catch (Exception ex)
            {
                return new CapturedExceptionResultTransporter<bool>(ex);
            }
        }
    }
}