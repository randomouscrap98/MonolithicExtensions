
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static MonolithicExtensions.Windows.RestartManager;

namespace MonolithicExtensions.Windows
{
    public static class RestartManager
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        public const int WINDOWS_XP_VERSION = 5;
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_MORE_DATA = 234;
        const int RmRebootReasonNone = 0;
        const int CCH_RM_MAX_APP_NAME = 255;
        const int CCH_RM_MAX_SVC_NAME = 63;

        public enum RM_SHUTDOWN_TYPE
        {
            None = 0,
            RmForceShutdown = 1,
            RmShutdownOnlyRegistered = 2
        }

        /// <summary>
        /// Flags indicating how to restart an application based on various criteria
        /// </summary>
        public enum ApplicationRestartFlag
        {
            NONE = 0,
            RESTART_NO_CRASH = 1,
            RESTART_NO_HANG = 2,
            RESTART_NO_PATCH = 4,
            RESTART_NO_REBOOT = 8
        }

        /// <summary>
        /// Windows Message system codes for the Restart Manager
        /// </summary>
        public enum WindowsMessageCode
        {
            WM_QUERYENDSESSION = 17,
            WM_ENDSESSION = 22
        }

        public enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        public enum RM_REBOOT_REASON
        {
            RmRebootReasonNone = 0,
            RmRebootReasonPermissionDenied = 1,
            RmRebootReasonSessionMismatch = 2,
            RmRebootReasonCriticalProcess = 4,
            RmRebootReasonCriticalService = 8,
            RmRebootReasonDetectedSelf = 16
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RestartManager.CCH_RM_MAX_APP_NAME + 1)]

            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RestartManager.CCH_RM_MAX_SVC_NAME + 1)]

            public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]

            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        public static extern int RmRegisterResources(uint pSessionHandle, UInt32 nFiles,
            string[] rgsFilenames, UInt32 nApplications, [In()] RM_UNIQUE_PROCESS[] rgApplications,
            UInt32 nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        public static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        public static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll")]
        public static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, 
            [In(), Out()] RM_PROCESS_INFO[] rgAffectedApps, out uint lpdwRebootReasons);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        public static extern int RmShutdown(uint pSessionHandle, uint lActionFlags, IntPtr fnStatus);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Auto)]
        public static extern int RmRestart(uint pSessionHandle, uint dwRestartFlags, IntPtr fnStatus);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegisterApplicationRestart(string pszCommandLine, int dwFlags); 

        /// <summary>
        /// Wraps the raw RmStartSession in an easier to use function. Starts a Restart Manager session.
        /// </summary>
        /// <param name="pSessionHandle"></param>
        /// <param name="dwSessionFlags"></param>
        /// <returns></returns>
        public static int RmStartSessionEx(ref uint pSessionHandle, int dwSessionFlags = 0)
        {
            return RmStartSession(out pSessionHandle, dwSessionFlags, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Wraps the raw RmRegisterResources in an easier to use function. 
        /// </summary>
        /// <param name="pSessionHandle"></param>
        /// <param name="filePaths"></param>
        /// <param name="processes"></param>
        /// <param name="serviceNames"></param>
        /// <returns></returns>
        public static int RmRegisterResourcesEx(ref uint pSessionHandle, IEnumerable<string> filePaths, IEnumerable<RM_UNIQUE_PROCESS> processes = null, IEnumerable<string> serviceNames = null)
        {
            if (processes == null)
                processes = new List<RM_UNIQUE_PROCESS>();
            if (serviceNames == null)
                serviceNames = new List<string>();

            return RmRegisterResources(pSessionHandle, Convert.ToUInt32(filePaths.Count()), filePaths.ToArray(), Convert.ToUInt32(processes.Count()), processes.ToArray(), Convert.ToUInt32(serviceNames.Count()), serviceNames.ToArray());

        }

        /// <summary>
        /// Wraps the raw RmGetList in an easier to use function. Gets the list of affected apps and stores it into rgAffectedApps.
        /// Attempts to retrieve processes up to maxAttempts times (due to inherent race condition present in restart manager)
        /// </summary>
        /// <param name="dwSessionHandle"></param>
        /// <param name="rgAffectedApps"></param>
        /// <param name="lpdwRebootReasons"></param>
        /// <returns></returns>
        public static int RmGetListEx(uint dwSessionHandle, ref List<RM_PROCESS_INFO> rgAffectedApps, ref RM_REBOOT_REASON lpdwRebootReasons, int maxAttempts = 5)
        {
            int attempts = 0;
            RM_PROCESS_INFO[] rawAffectedApps = null;
            uint affectedApps = 0;
            uint rawAffectedAppsSize = 0;
            int result = 0;

            //Only attempt to pull processes until we reach the maximum attempts. There is an inherent race condition: RmGetList returns
            //ERROR_MORE_DATA if you did not supply an array large enough to hold all the processes. HOWEVER, if the number of processes
            //increases before you call RmGetList again with the larger array, it will no longer be large enough to hold it. This can repeat
            //forever if processes keep increasing (for some reason). Thus, we only retry a set amount of times.
            do
            {
                uint lpdwInt = (uint)lpdwRebootReasons;
                result = RmGetList(dwSessionHandle, out affectedApps, ref rawAffectedAppsSize, rawAffectedApps, out lpdwInt);

                if (result == ERROR_MORE_DATA)
                {
                    //If we didn't make a big enough array to hold all the processes, increase the size of the array.
                    rawAffectedApps = new RM_PROCESS_INFO[Convert.ToInt32(affectedApps) + 1];
                    rawAffectedAppsSize = Convert.ToUInt32(rawAffectedApps.Length);
                }
                else if (result == ERROR_SUCCESS)
                {
                    //Otherwise, if everything is fine, fill the provided list with the affected apps and be on our way
                    if (rawAffectedApps != null)
                    {
                        rgAffectedApps = new List<RM_PROCESS_INFO>(rawAffectedApps.Take(Convert.ToInt32(affectedApps)));
                    }
                    break; // TODO: might not be correct. Was : Exit Do
                }
                else
                {
                    //Oof, something we don't recognize? It's probably an error; just exit so the user knows the results.
                    break; // TODO: might not be correct. Was : Exit Do
                }

                attempts += 1;

            } while (attempts < maxAttempts);

            return result;
        }

        /// <summary>
        /// Wraps the raw RegisterApplicationRestart function to make life a bit easier
        /// </summary>
        /// <param name="pszCommandLine"></param>
        /// <param name="dwFlags"></param>
        /// <returns></returns>
        public static uint RegisterApplicationRestartEx(string pszCommandLine = "", int dwFlags = 0)
        {
            return RegisterApplicationRestart(pszCommandLine, dwFlags);
        }

        /// <summary>
        /// Convert a list of process info returned by a Restart Manager function into a list of proper .Net Process structures.
        /// Processes that are no longer valid or which cannot be converted are simply not included in the result.
        /// </summary>
        /// <param name="rmProcessInfos"></param>
        /// <returns></returns>
        public static List<Process> RmProcessToNetProcess(IEnumerable<RM_PROCESS_INFO> rmProcessInfos)
        {
            List<Process> processes = new List<Process>();

            // Enumerate all of the results And add them to the 
            // list to be returned
            foreach (var processInfo in rmProcessInfos)
            {
                //processInfo = processInfo_loopVariable;
                try
                {
                    processes.Add(Process.GetProcessById(processInfo.Process.dwProcessId));
                }
                catch (ArgumentException Ex)
                {
                    // catch the error -- in case the process Is no longer running
                }
            }

            return processes;
        }

        public static bool IsRestartManagerSupported()
        {
            return System.Environment.OSVersion.Version.Major > WINDOWS_XP_VERSION;
        }

        /// <summary>
        /// Find what processes (including services) have a lock on the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Process> WhoIsLocking(string path)
        {
            return WhoIsLocking(new List<string>(){ path });
        }

        /// <summary>
        /// Find out what process(es) have a lock on the given files.
        /// </summary>
        /// <param name="paths">Paths of the files.</param>
        /// <returns>Processes locking the files</returns>
        public static List<Process> WhoIsLocking(IEnumerable<string> paths)
        {

            RestartManagerSession restartSession = new RestartManagerSession();
            RM_REBOOT_REASON rebootReasons = default(RM_REBOOT_REASON);

            //All this crap can throw an exception. Let's hope the caller handles these exceptions at some point.
            restartSession.StartSession();
            //ThreadingServices.WaitOnAction(
            //    Sub()
            //        If restartSession.SessionHandle = 0 Then Throw New Exception("Waiting for session handle to become valid")
            //    End Sub, TimeSpan.FromSeconds(5))

            //Sub() restartSession.RegisterResources(paths), TimeSpan.FromSeconds(5))
            restartSession.RegisterResources(paths);
            var rmProcesses = restartSession.GetList(ref rebootReasons);
            var realProcesses = RmProcessToNetProcess(rmProcesses);
            restartSession.EndSession();

            return realProcesses;

            #region "OldMethod"
            //Adapted from http://stackoverflow.com/questions/317071/how-do-i-find-out-which-process-is-locking-a-file-using-net
            //Also adapted from https://mdetras.com/category/vb-net/
            // http//msdn.microsoft.com/en-us/library/windows/desktop/aa373661(v=vs.85).aspx
            // http://wyupdate.googlecode.com/svn-history/r401/trunk/frmFilesInUse.cs (no copyright in code at time of viewing)

            //Dim handle As UInteger
            //Dim key As String = Guid.NewGuid().ToString()
            //Dim processes As New List(Of Process)

            //Dim result As Integer = RmStartSession(handle, 0, key)

            //If (result <> 0) Then Throw New Exception("Could not begin restart session.  Unable to determine file locker.")

            //Try

            //    Dim pnProcInfoNeeded As UInteger = 0,
            //    pnProcInfo As UInteger = 0,
            //    lpdwRebootReasons As UInteger = RmRebootReasonNone

            //    Dim resources = New String() {path}   ' Just checking On one resource.

            //    result = RmRegisterResources(handle, CType(resources.Length, UInteger), resources, 0, Nothing, 0, Nothing)
            //    If (result <> 0) Then Throw New Exception("Could not register resource.")

            //    'Note there's a race condition here -- the first call to RmGetList() returns
            //    'the total number of process. However, when we call RmGetList() again to get
            //    'the actual processes this number may have increased.
            //    result = RmGetList(handle, pnProcInfoNeeded, pnProcInfo, Nothing, lpdwRebootReasons)

            //    If (result = ERROR_MORE_DATA) Then

            //        ' Create an array to store the process results
            //        Dim processInfo As RM_PROCESS_INFO() = New RM_PROCESS_INFO(CType(pnProcInfoNeeded, Integer)) {}
            //        pnProcInfo = pnProcInfoNeeded

            //        ' Get the list
            //        result = RmGetList(handle, pnProcInfoNeeded, pnProcInfo, processInfo, lpdwRebootReasons)

            //        If (result = 0) Then

            //            processes = New List(Of Process)(CType(pnProcInfo, Integer))

            //            ' Enumerate all of the results And add them to the 
            //            ' list to be returned
            //            For i = 0 To pnProcInfo - 1
            //                Try
            //                    processes.Add(Process.GetProcessById(processInfo(CType(i, Integer)).Process.dwProcessId))
            //                Catch Ex As ArgumentException
            //                    ' catch the error -- in case the process Is no longer running
            //                End Try
            //            Next

            //        Else
            //            Throw New Exception("Could not list processes locking resource.")
            //        End If

            //    ElseIf (result <> 0) Then
            //        Throw New Exception("Could not list processes locking resource. Failed to get size of result.")
            //    End If

            //Finally
            //    RmEndSession(handle)
            //End Try

            //Return processes
            #endregion

        }

    }

    /// <summary>
    /// Represents an exception that occurred for a restart manager session
    /// </summary>
    public class RmSessionException : Exception
    {
        public int RawError { get; }

        public RmSessionException(string Message, int RawError = -1) : base(Message)
        {
            this.RawError = RawError;
        }

        public RmSessionException(string Message, Exception InnerException, int RawError = -1) : base(Message, InnerException)
        {
            this.RawError = RawError;
        }
    }

    /// <summary>
    /// A wrapper for interacting with the restart manager. A single RestartManagerSession object will represent a single
    /// session of the Restart Manager, and will manage the session handle for you.
    /// </summary>
    public class RestartManagerSession
    {
        private bool Started { get; set; } = false;
        protected Portable.Logging.ILogger Logger { get; }

        public uint SessionHandle = 0; // { get; set; }
        public Guid SessionKey { get; } = Guid.NewGuid();

        public RestartManagerSession()
        {
            Logger = Portable.Logging.LogServices.CreateLoggerFromDefault(this.GetType());
        }

        /// <summary>
        /// Perfroms the startup, registration, and shutdown for the given files. Useful if you don't care
        /// about any of the intermediate steps and just want stuff shut down (and are OK with the entire thing failing
        /// if any steps fail)
        /// </summary>
        /// <param name="files"></param>
        public void EasyShutdown(IEnumerable<string> files)
        {
            RestartManager.RM_REBOOT_REASON rebootreason = default(RestartManager.RM_REBOOT_REASON);
            StartSession();
            RegisterResources(files);
            var processesToRestart = RestartManager.RmProcessToNetProcess(GetList(ref rebootreason));

            if (rebootreason != RM_REBOOT_REASON.RmRebootReasonNone)
            {
                string message = "There are processes or services which the RestartManager can't shut down!";
                Logger.Warn("RestartManagerSession.EasyShutdown encountered error: " + message);
                throw new RmSessionException(message);
            }

            Logger.Info("The following processes will be affected by the RestartManager: " + string.Join(", ", processesToRestart.Select(x => x.ProcessName)));

            try
            {
                Shutdown();
            }
            catch (Exception ex)
            {
                EndSession();
                throw ex;
            }

            Logger.Info("RestartManager successfully shut down the above processes");

        }

        public void EasyRestart()
        {
            Restart();
            EndSession();
            Logger.Info("RestartManager successfully restart all processes");
        }

        /// <summary>
        /// Start a restart manager session
        /// </summary>
        /// <param name="dwSessionFlags"></param>
        public void StartSession(int dwSessionFlags = 0)
        {
            Logger.Trace("Starting RMSession " + SessionKey.ToString());
            var result = RestartManager.RmStartSession(out SessionHandle, dwSessionFlags, SessionKey.ToString());
            if (result != RestartManager.ERROR_SUCCESS)
                throw new RmSessionException("Could not start session", result);
            Started = true;
        }

        /// <summary>
        /// End the restart manager session
        /// </summary>
        public void EndSession()
        {
            Logger.Trace("Ending RMSession " + SessionKey.ToString());
            if (!Started)
                throw new InvalidOperationException("The RM Session has not been started yet!");
            dynamic result = RestartManager.RmEndSession(SessionHandle);
            if (result != RestartManager.ERROR_SUCCESS)
                throw new RmSessionException("Could not end session!", result);
            SessionHandle = 0;
        }

        /// <summary>
        /// Register the given resources with the restart manager.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="processes"></param>
        /// <param name="serviceNames"></param>
        public void RegisterResources(IEnumerable<string> filePaths, IEnumerable<RM_UNIQUE_PROCESS> processes = null, IEnumerable<string> serviceNames = null)
        {
            if (processes == null) processes = new List<RM_UNIQUE_PROCESS>();
            if (serviceNames == null) serviceNames = new List<string>();
            Logger.Trace(string.Format("Registering {0} files, {1} processes, and {2} services with RMSession {3}", filePaths.Count(), processes.Count(), serviceNames.Count(), SessionKey));
            if (!Started) throw new InvalidOperationException("The RM Session has not been started yet!");
            var result = RestartManager.RmRegisterResourcesEx(ref SessionHandle, filePaths, processes, serviceNames);
            if (result != RestartManager.ERROR_SUCCESS) throw new RmSessionException("Could not register resources", result);
        }

        /// <summary>
        /// Try to get the list of processes affected by the registered resources
        /// </summary>
        /// <param name="lpdwRebootReasons"></param>
        /// <returns></returns>
        public virtual List<RM_PROCESS_INFO> GetList(ref RM_REBOOT_REASON lpdwRebootReasons)
        {
            Logger.Trace("Getting list of processes for RMSession " + SessionKey.ToString());
            if (!Started) throw new InvalidOperationException("The RM Session has not been started yet!");
            List<RM_PROCESS_INFO> processes = new List<RM_PROCESS_INFO>();
            var result = RestartManager.RmGetListEx(SessionHandle, ref processes, ref lpdwRebootReasons);
            if (result != RestartManager.ERROR_SUCCESS) throw new RmSessionException("Could not retrieve process list", result);
            return processes;
        }

        /// <summary>
        /// Shutdown the processes and services associated with the registered files.
        /// </summary>
        /// <param name="actionFlags"></param>
        public virtual void Shutdown(uint actionFlags = 0)
        {
            Logger.Trace("Shutting down RMSession " + SessionKey.ToString());
            if (!Started) throw new InvalidOperationException("The RM Session has not been started yet!");
            var result = RestartManager.RmShutdown(SessionHandle, actionFlags, IntPtr.Zero);
            if (result != RestartManager.ERROR_SUCCESS) throw new RmSessionException("Could not shutdown processes or services", result);
        }

        /// <summary>
        /// Restart the processes and services associated with the registered files.
        /// </summary>
        /// <param name="restartFlags"></param>
        public virtual void Restart(uint restartFlags = 0)
        {
            Logger.Trace("Restarting RMSession " + SessionKey.ToString());
            if (!Started) throw new InvalidOperationException("The RM Session has not been started yet!");
            var result = RestartManager.RmRestart(SessionHandle, restartFlags, IntPtr.Zero);
            if (result != RestartManager.ERROR_SUCCESS) throw new RmSessionException("Could not restart processes or services!", result);
        }
    }

    /// <summary>
    /// Provides extra functionality to the restart manager session; mostly for allowing manual
    /// restarting of specified non-RM aware processes.
    /// </summary>
    public class RestartManagerExtendedSession : RestartManagerSession
    {
        private List<Process> ProcessesToManuallyShutdown { get; set; } = new List<Process>();
        private List<Tuple<IntPtr, string>> ProcessesToManuallyRestart { get; set; } = new List<Tuple<IntPtr, string>>();

        /// <summary>
        /// Processes which should not be handled by the restart manager but which should be killed manually.
        /// </summary>
        /// <returns></returns>
        public List<string> ManualRestartProcesses { get; set; } = new List<string>();

        /// <summary>
        /// Retrieve the list of processes that are holding onto registered resources. ALSO internally sets
        /// the list of processes to manually restart
        /// </summary>
        /// <param name="lpdwRebootReasons"></param>
        /// <returns></returns>
        public override List<RM_PROCESS_INFO> GetList(ref RM_REBOOT_REASON lpdwRebootReasons)
        {
            Logger.Trace(string.Format("Getting RMSession {0} process list with {1} manual processes to restart", SessionKey, ManualRestartProcesses.Count));
            var processes = base.GetList(ref lpdwRebootReasons);
            var netProcesses = RestartManager.RmProcessToNetProcess(processes);
            ProcessesToManuallyShutdown = netProcesses.Where(x => ManualRestartProcesses.Select(y => y.Replace(".exe", "")).Contains(x.ProcessName)).ToList();
            return processes;
        }

        /// <summary>
        /// Shutdown all registered processes, including processes marked for manual shutdown (if necessary).
        /// <remarks>To ensure process restart success, the system MUST be able to query the user token from the process (you must be Administrator/LocalSystem).
        /// Otherwise, processes could restart as the wrong user, breaking the functionality of the process.</remarks>
        /// </summary>
        /// <param name="actionFlags"></param>
        public override void Shutdown(uint actionFlags = 0)
        {
            Logger.Trace(string.Format("Shutting down RMSession {0} with {1} manual processes found to shutdown", SessionKey, ProcessesToManuallyShutdown.Count));
            if (ProcessesToManuallyRestart.Count > 0) Logger.Warn("There are pending processes in the manual restart queue! Are you shutting down again?");
            foreach (Process manualShutdownProcess in ProcessesToManuallyShutdown)
            {
                IntPtr userToken = IntPtr.Zero;
                if (!WindowsAccountServices.GetUserTokenFromProcess(manualShutdownProcess.Id, ref userToken))
                {
                    throw new InvalidOperationException("Could not retrieve the user token for manual shutdown process " + manualShutdownProcess.ProcessName);
                }
                ProcessesToManuallyRestart.Add(Tuple.Create(userToken, manualShutdownProcess.MainModule.FileName));
            }
            ProcessesToManuallyShutdown.ForEach(x => x.Kill());
            ProcessesToManuallyShutdown.Clear();
            base.Shutdown(actionFlags);
        }

        public override void Restart(uint restartFlags = 0)
        {
            base.Restart(restartFlags);
            Logger.Trace(string.Format("Restarting RMSession {0} with {1} manual processes found to restart", SessionKey, ProcessesToManuallyRestart.Count));
            foreach (Tuple<IntPtr, string> processInfo in ProcessesToManuallyRestart)
            {
                try
                {
                    if (!WindowsAccountServices.StartProcessFromUserToken(processInfo.Item1, processInfo.Item2))
                    {
                        throw new InvalidOperationException("Could not restart process " + processInfo.Item2 + " using previously defined user token!");
                    }
                }
                finally
                {
                    WindowsAccountServices.CloseHandle(processInfo.Item1);
                }
            }
            ProcessesToManuallyRestart.Clear();
        }

        /// <summary>
        /// Try to get the list of processes affected by the registered resources AS .net processes
        /// </summary>
        /// <param name="lpdwRebootReasons"></param>
        /// <returns></returns>
        public virtual List<Process> GetListAsProcesses(ref RM_REBOOT_REASON lpdwRebootReasons)
        {
            return RestartManager.RmProcessToNetProcess(GetList(ref lpdwRebootReasons));
        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
