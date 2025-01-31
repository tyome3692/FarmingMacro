﻿using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FarmingMacro
{
    sealed partial class KeyboardHook
    {
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static partial IntPtr SetWindowsHookExW(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UnhookWindowsHookEx(IntPtr hhk);

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static partial IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.UserDirectories)]
        private static partial IntPtr GetModuleHandleW(string lpModuleName);

        const int WH_KEYBOARD_LL = 0x000D;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_SYSKEYDOWN = 0x0104;
        const int WM_SYSKEYUP = 0x0105;

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class KBDLLHOOKSTRUCT
        {
            internal uint vkCode;
            internal uint scanCode;
            internal KBDLLHOOKSTRUCTFlags flags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        [Flags]
        internal enum KBDLLHOOKSTRUCTFlags : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_SCANCODE = 0x0008,
            KEYEVENTF_UNICODE = 0x0004,
        }

        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private KeyboardProc? proc;
        private IntPtr hookId = IntPtr.Zero;

        internal void Hook()
        {
            if (hookId == IntPtr.Zero)
            {
                proc = HookProcedure;
                using (var curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess?.MainModule ?? throw new ArgumentNullException(nameof(curProcess)))
                {
                    if (string.IsNullOrEmpty(curModule.ModuleName))
                        throw new ArgumentNullException(nameof(curModule));
                    hookId = SetWindowsHookExW(WH_KEYBOARD_LL, proc, GetModuleHandleW(curModule.ModuleName), 0);
                }
            }
        }

        internal void UnHook()
        {
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        internal IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
            {
                var kb = (KBDLLHOOKSTRUCT?)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var vkCode = (int)(kb?.vkCode ?? throw new ArgumentNullException(nameof(lParam)));
                OnKeyDownEvent(vkCode);
            }
            else if (nCode >= 0 && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
            {
                var kb = (KBDLLHOOKSTRUCT?)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                var vkCode = (int)(kb?.vkCode ?? throw new ArgumentNullException(nameof(lParam)));
                OnKeyUpEvent(vkCode);
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        internal delegate void KeyEventHandler(object sender, KeyEventArgs e);
        internal event KeyEventHandler? KeyDownEvent;
        internal event KeyEventHandler? KeyUpEvent;

        void OnKeyDownEvent(int keyCode)
        {
            KeyDownEvent?.Invoke(this, new KeyEventArgs(keyCode));
        }
        void OnKeyUpEvent(int keyCode)
        {
            KeyUpEvent?.Invoke(this, new KeyEventArgs(keyCode));
        }

    }

    internal class KeyEventArgs : EventArgs
    {
        internal int KeyCode { get; }

        internal KeyEventArgs(int keyCode)
        {
            KeyCode = keyCode;
        }
    }
}
