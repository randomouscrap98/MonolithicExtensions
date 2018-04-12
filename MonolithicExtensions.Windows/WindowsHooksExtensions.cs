
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonolithicExtensions.Windows
{
    public static class WindowsHookUtilities
    {
        public enum HookID
        {
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4
        }

        public delegate IntPtr HookProcess(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProcess lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static Dictionary<IntPtr, HookProcess> CurrentlySetHooks = new Dictionary<IntPtr, HookProcess>();
        private static object CurrentHooksLock = new object();

        /// <summary>
        /// Use this to generate simple hook handlers if all you need to do is perform a simple action
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static HookProcess GenerateEasyHook(Action<IntPtr, IntPtr> handler)
        {
            return (int nCode, IntPtr wParam, IntPtr lParam) =>
            {
                if (nCode >= 0)
                {
                    handler.Invoke(wParam, lParam);
                }
                return WindowsHookUtilities.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
            };
        }

        /// <summary>
        /// Attach the given handler to intercept the given message types
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static IntPtr SetHook(HookID messageType, HookProcess handler)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule currentModule = currentProcess.MainModule)
                {
                    var hook = SetWindowsHookEx(Convert.ToInt32(messageType), handler, GetModuleHandle(currentModule.ModuleName), 0);
                    lock (CurrentHooksLock)
                    {
                        CurrentlySetHooks.Add(hook, handler);
                    }
                    return hook;
                }
            }
        }

        /// <summary>
        /// Remove the given message handler.
        /// </summary>
        /// <param name="hookID"></param>
        /// <returns></returns>
        public static bool RemoveHook(IntPtr hookID)
        {
            lock (CurrentHooksLock)
            {
                CurrentlySetHooks.Remove(hookID);
            }
            return UnhookWindowsHookEx(hookID);
        }
    }


    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
