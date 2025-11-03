using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core.Helpers
{
    public static class KeyboardBlocker
    {
        private static IntPtr _hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc _proc;
        private static bool _isBlocking = false;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [Flags]
        private enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,  // Flag cho injected keys
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Bật chặn bàn phím vật lý (physical keys), nhưng vẫn cho phép phím injected từ AutoHotKey.
        /// </summary>
        public static void BlockPhysicalKeyboard()
        {
            if (_isBlocking) return;

            _proc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
            _isBlocking = true;
            Console.WriteLine("Physical keyboard blocked.");
        }

        /// <summary>
        /// Tắt chặn bàn phím vật lý.
        /// </summary>
        public static void UnblockPhysicalKeyboard()
        {
            if (!_isBlocking) return;

            UnhookWindowsHookEx(_hookId);
            _isBlocking = false;
            Console.WriteLine("Physical keyboard unblocked.");
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                KBDLLHOOKSTRUCT kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);

                // Kiểm tra nếu là injected key (từ AHK hoặc simulated)
                bool isInjected = (kbStruct.flags & KBDLLHOOKSTRUCTFlags.LLKHF_INJECTED) != 0;

                if (!isInjected)
                {
                    // Là physical key: Chặn bằng return 1
                    return new IntPtr(1);
                }
            }

            // Cho phép injected keys hoặc các event khác đi qua
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
