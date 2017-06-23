
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonolithicExtensions.Windows
{
    /// <summary>
    /// <remarks>Taken from https://github.com/murrayju/CreateProcessAsUser after much alteration.</remarks>
    /// </summary>
    public static class WindowsAccountServices
    {
        private static Portable.Logging.ILogger Logger = Portable.Logging.LogServices.CreateLoggerFromDefault(typeof(WindowsAccountServices)); 
        //log4net.ILog Logger { get; }

        #region "Win32 Constants"

        private const int CREATE_UNICODE_ENVIRONMENT = 0x400;

        private const int CREATE_NO_WINDOW = 0x8000000;

        private const int CREATE_NEW_CONSOLE = 0x10;

        private const uint INVALID_SESSION_ID = 0xffffffffu;

        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
        private const uint STANDARD_RIGHTS_REQUIRED = 0xf0000;
        private const uint STANDARD_RIGHTS_READ = 0x20000;
        private const uint TOKEN_ASSIGN_PRIMARY = 0x1;
        private const uint TOKEN_DUPLICATE = 0x2;
        private const uint TOKEN_IMPERSONATE = 0x4;
        private const uint TOKEN_QUERY = 0x8;
        private const uint TOKEN_QUERY_SOURCE = 0x10;
        private const uint TOKEN_ADJUST_PRIVILEGES = 0x20;
        private const uint TOKEN_ADJUST_GROUPS = 0x40;
        private const uint TOKEN_ADJUST_DEFAULT = 0x80;
        private const uint TOKEN_ADJUST_SESSIONID = 0x100;
        private const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        private const uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT | TOKEN_ADJUST_SESSIONID);
        #endregion

        #region "DllImports"

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, 
            IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandle, uint dwCreationFlags, 
            IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        private static extern bool DuplicateTokenEx(IntPtr ExistingTokenHandle, uint dwDesiredAccess,
            IntPtr lpThreadAttributes, int TokenType, int ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        [DllImport("userenv.dll", SetLastError = true)]
        private static extern bool CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        [DllImport("userenv.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyEnvironmentBlock(IntPtr lpEnvironment);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hSnapshot);

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("Wtsapi32.dll")]
        private static extern uint WTSQueryUserToken(uint SessionId, ref IntPtr phToken);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern int WTSEnumerateSessions(IntPtr hServer, int Reserved, int Version, ref IntPtr ppSessionInfo, ref int pCount);

        #endregion

        #region "Win32 Structs"

        [Flags()]
        public enum ProcessAccessFlags : uint
        {
            All = 0x1f0fff,
            Terminate = 0x1,
            CreateThread = 0x2,
            VirtualMemoryOperation = 0x8,
            VirtualMemoryRead = 0x10,
            VirtualMemoryWrite = 0x20,
            DuplicateHandle = 0x40,
            CreateProcess = 0x80,
            SetQuota = 0x100,
            SetInformation = 0x200,
            QueryInformation = 0x400,
            QueryLimitedInformation = 0x1000,
            Synchronize = 0x100000
        }

        private enum SW
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        private enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        private enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public readonly UInt32 SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string pWinStationName;
            public readonly WTS_CONNECTSTATE_CLASS State;
        }

        #endregion

        /// <summary>
        /// Gets the user token from the currently active session
        /// </summary>
        public static bool GetSessionUserToken(ref IntPtr phUserToken)
        {
            Logger.Trace("GetSessionUserToken called");

            bool bResult = false;
            IntPtr hImpersonationToken = IntPtr.Zero;
            var activeSessionId = INVALID_SESSION_ID;
            IntPtr pSessionInfo = IntPtr.Zero;
            int sessionCount = 0;

            // Get a handle to the user access token for the current active session.
            if ((WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount) != 0))
            {
                var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                var current = pSessionInfo;

                for (int i = 0; i <= sessionCount - 1; i++)
                {
                    var si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO));
                    current += arrayElementSize;

                    if ((si.State == WTS_CONNECTSTATE_CLASS.WTSActive))
                    {
                        activeSessionId = si.SessionID;
                        Logger.Trace("Found user session ID: " + activeSessionId);
                    }
                }
            }

            // If enumerating did Not work, fall back to the old method
            if ((activeSessionId == INVALID_SESSION_ID))
            {
                activeSessionId = WTSGetActiveConsoleSessionId();
                Logger.Trace("Could not find user session ID by enumerating sessions. Session ID retrieved from console: " + activeSessionId);
            }

            // Convert the impersonation token to a primary token
            if ((WTSQueryUserToken(activeSessionId, ref hImpersonationToken) != 0))
            {
                bResult = DuplicateTokenEx(hImpersonationToken, 0, IntPtr.Zero, Convert.ToInt32(SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation), Convert.ToInt32(TOKEN_TYPE.TokenPrimary), ref phUserToken);
                Logger.Trace("Successfully ran user session token duplication. Result: " + bResult);
                CloseHandle(hImpersonationToken);
            }
            else
            {
                Logger.Trace("Could not run WTSQueryUserToken on activeSessionId. Do you have enough privileges?");
            }

            return bResult;
        }

        public static bool StartProcessAsCurrentUser(string appPath, string cmdLine = null, string workDir = null, bool visible = true)
        {
            IntPtr hUserToken = IntPtr.Zero;
            try
            {
                if ((!GetSessionUserToken(ref hUserToken)))
                    throw new Exception("StartProcessAsCurrentUser: GetSessionUserToken failed.");
                return StartProcessFromUserToken(hUserToken, appPath, cmdLine, workDir, visible);
            }
            finally
            {
                CloseHandle(hUserToken);
            }
        }

        /// <summary>
        /// Get a pointer to the user token for the given process.
        /// </summary>
        /// <param name="processID"></param>
        /// <param name="userToken"></param>
        /// <returns></returns>
        public static bool GetUserTokenFromProcess(int processID, ref IntPtr userToken)
        {
            Logger.Trace("GetUserTokenFromProcess called with processID " + processID);
            IntPtr processHandle = OpenProcess(ProcessAccessFlags.QueryInformation, false, processID);
            //ProcessAccessFlags.All, False, processID)
            if (processHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not get process handle for process ID " + processID);
            }
            var result = OpenProcessToken(processHandle, (int)TOKEN_ALL_ACCESS, ref userToken);
            CloseHandle(processHandle);
            return result;
        }

        /// <summary>
        /// Wrapper for pinvoke call CreateProcessAsUser(). Starts the given process with the given user token.
        /// </summary>
        /// <param name="hUserToken"></param>
        /// <param name="appPath"></param>
        /// <param name="cmdLine"></param>
        /// <param name="workDir"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        public static bool StartProcessFromUserToken(IntPtr hUserToken, string appPath, string cmdLine = null, string workDir = null, bool visible = true)
        {
            Logger.Trace("StartProcessFromUserToken called with token " + hUserToken.ToInt32() + " and app " + appPath);

            var startInfo = new STARTUPINFO();
            var procInfo = new PROCESS_INFORMATION();
            IntPtr pEnv = IntPtr.Zero;
            //Dim iResultOfCreateProcessAsUser As Integer

            startInfo.cb = Marshal.SizeOf(typeof(STARTUPINFO));

            try
            {
                uint dwCreationFlags = Convert.ToUInt32(CREATE_UNICODE_ENVIRONMENT | (visible ? CREATE_NEW_CONSOLE : CREATE_NO_WINDOW));
                startInfo.wShowWindow = Convert.ToInt16(visible ? SW.SW_SHOW : SW.SW_HIDE);
                startInfo.lpDesktop = "winsta0\\default";

                if ((!CreateEnvironmentBlock(ref pEnv, hUserToken, false)))
                {
                    throw new Exception("StartProcessAsCurrentUser: CreateEnvironmentBlock failed.");
                }

                Logger.Trace("StartProcessAsCurrentUser CreateEnvironmentBlock success!");

                // Application Name
                // Command() Line
                // Working directory
                if ((!CreateProcessAsUser(hUserToken, appPath, cmdLine, IntPtr.Zero, IntPtr.Zero, false, dwCreationFlags, pEnv, workDir, ref startInfo,
                ref procInfo)))
                {
                    Logger.Warn("CreateProcessAsUser failed. Code: " + Marshal.GetLastWin32Error());
                    throw new Exception("StartProcessAsCurrentUser: CreateProcessAsUser failed.");
                }

                Logger.Trace("StartProcessAsCurrentUser CreateProcessAsUser success!");
            }
            finally
            {
                if ((pEnv != IntPtr.Zero))
                {
                    DestroyEnvironmentBlock(pEnv);
                }
                CloseHandle(procInfo.hThread);
                CloseHandle(procInfo.hProcess);
            }

            Logger.Trace("Returning true from StartProcessAsCurrentUser!");
            return true;
        }
    }

    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
