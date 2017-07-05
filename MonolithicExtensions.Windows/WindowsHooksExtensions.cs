
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

    ///' <summary>
    ///' 
    ///' </summary>
    ///' <remarks>Some of this code was taken from 
    ///' https://social.msdn.microsoft.com/Forums/en-US/94b1598f-79ce-4c70-8851-86aff6d9b12f/capturing-the-print-screen-key?forum=Vsexpressvcs
    ///' </remarks>
    //Public Module KeyboardHookUtilities

    //    Private Const WH_KEYBOARD_LL As Integer = 13
    //    Private Const WM_KEYDOWN As Integer = &H100
    //    Private Const VK_F1 As Integer = &H70

    //    Public Const WM_SYSKEYDOWN As Integer = 260

    //    Public Delegate Function LowLevelKeyboardProc(nCode As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr

    //        /// <summary>
    //        /// Call this function to add your keyboard hook. You can generate keyboard hooks using the functions provided in this class.
    //        /// </summary>
    //        /// <param name="proc"></param>
    //        /// <returns></returns>
    //        Public Static IntPtr SetHook(LowLevelKeyboardProc proc)
    //        {
    //            Using (Process curProcess = Process.GetCurrentProcess())
    //            Using (ProcessModule curModule = curProcess.MainModule)
    //            {
    //                Return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    //            }
    //        }

    //        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //        Private Static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    //        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        Public Static extern bool UnhookWindowsHookEx(IntPtr hhk);

    //        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //        Public Static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    //        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //        Private Static extern IntPtr GetModuleHandle(String lpModuleName);

    //        /// <summary>
    //        /// Create a printscreen hook that performs the given action when the printscreen key Is pressed.
    //        /// </summary>
    //        /// <param name="OnPrintScreen"></param>
    //        /// <returns></returns>
    //        Public Static KeyboardHookUtilities.LowLevelKeyboardProc GeneratePrintScreenHook(Action OnPrintScreen)
    //        {
    //            Return (int nCode, IntPtr wParam, IntPtr lParam) => HookCallback(nCode, wParam, lParam, Keys.PrintScreen, Keys.None, OnPrintScreen);
    //        }

    //        /// <summary>
    //        /// Generic keyboard handler which performs the given action when the given key combination And modifiers are pressed down.
    //        /// </summary>
    //        /// <param name="nCode"></param>
    //        /// <param name="wParam"></param>
    //        /// <param name="lParam"></param>
    //        /// <param name="keyCombination"></param>
    //        /// <param name="modifiers"></param>
    //        /// <param name="OnPrintScreen"></param>
    //        /// <returns></returns>
    //        Private Static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam, Keys keyCombination, Keys modifiers, Action OnKeyAction)
    //        {
    //            If (nCode >= 0)
    //            {
    //                Keys number = (Keys)Marshal.ReadInt32(lParam);
    //                If (wParam == (IntPtr)260 && number == keyCombination && (modifiers == Keys.None || modifiers == Control.ModifierKeys))
    //                    OnKeyAction();
    //            }
    //            Return KeyboardHookUtilities.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    //        }
    //    }

    //End Module
    ///// <summary>
    ///// The main entry point for the application.
    ///// </summary>
    //[STAThread]
    //Static void Main()
    //{
    //    Application.EnableVisualStyles();
    //    Application.SetCompatibleTextRenderingDefault(false);
    //    _hookID = SetHook(_proc);
    //    Application.Run(New Form1());
    //    UnhookWindowsHookEx(_hookID);
    //}


    //=======================================================
    //Service provided by Telerik (www.telerik.com)
    //Conversion powered by NRefactory.
    //Twitter: @telerik
    //Facebook: facebook.com/telerik
    //=======================================================
}
