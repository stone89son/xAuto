using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public class XAutoNative
    {


        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        //public const int WM_SETTEXT = 0x000C;
        //public const int WM_GETTEXT = 0x000D;
        //public const int WM_GETTEXTLENGTH = 0x000E;
        //public const int WM_SETFOCUS = 0x0007;

        //public const uint EM_SETSEL = 0x00B1;        // chọn đoạn văn bản
        //public const uint EM_REPLACESEL = 0x00C2;    // chèn text vào vị trí đang chọn


        //// <summary>
        //// KeyBoard helper
        //[StructLayout(LayoutKind.Sequential)]
        //public struct INPUT
        //{
        //    public uint type;
        //    public InputUnion u;
        //}

        //[StructLayout(LayoutKind.Explicit)]
        //public struct InputUnion
        //{
        //    [FieldOffset(0)] public KEYBDINPUT ki;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct KEYBDINPUT
        //{
        //    public ushort wVk;
        //    public ushort wScan;
        //    public uint dwFlags;
        //    public uint time;
        //    public IntPtr dwExtraInfo;
        //}

        //public const uint INPUT_KEYBOARD = 1;
        //public const uint KEYEVENTF_UNICODE = 0x0004;
        //public const uint KEYEVENTF_KEYUP = 0x0002;

        const uint INPUT_MOUSE = 0;
        const uint INPUT_KEYBOARD = 1;
        const uint INPUT_HARDWARE = 2;

        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;

        const int SW_SHOW = 5;

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo; // use IntPtr so size matches ULONG_PTR
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public static void SendText(string text)
        {
            INPUT[] inputs = new INPUT[text.Length * 2];
            for (int i = 0; i < text.Length; i++)
            {
                // key down
                inputs[i * 2].type = INPUT_KEYBOARD;
                inputs[i * 2].u.ki.wScan = text[i];
                inputs[i * 2].u.ki.dwFlags = KEYEVENTF_UNICODE;
                inputs[i * 2].u.ki.wVk = 0;
                inputs[i * 2].u.ki.time = 0;
                inputs[i * 2].u.ki.dwExtraInfo = IntPtr.Zero;

                // key up
                inputs[i * 2 + 1].type = INPUT_KEYBOARD;
                inputs[i * 2 + 1].u.ki.wScan = text[i];
                inputs[i * 2 + 1].u.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
                inputs[i * 2 + 1].u.ki.wVk = 0;
                inputs[i * 2 + 1].u.ki.time = 0;
                inputs[i * 2 + 1].u.ki.dwExtraInfo = IntPtr.Zero;
            }

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static bool SendUnicodeText(string text)
        {
            int size = Marshal.SizeOf(typeof(INPUT));
            Console.WriteLine("Marshal.SizeOf(INPUT) = " + size);

            foreach (char ch in text)
            {
                INPUT down = new INPUT();
                down.type = INPUT_KEYBOARD;
                down.u = new InputUnion();
                down.u.ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = (ushort)ch,
                    dwFlags = KEYEVENTF_UNICODE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                };

                INPUT up = new INPUT();
                up.type = INPUT_KEYBOARD;
                up.u = new InputUnion();
                up.u.ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = (ushort)ch,
                    dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                };

                INPUT[] inputs = new INPUT[] { down, up };
                uint sent = SendInput((uint)inputs.Length, inputs, size);
                if (sent == 0)
                {
                    int err = Marshal.GetLastWin32Error();
                    Console.WriteLine($"SendInput failed for '{ch}' (0x{(int)ch:X}) -> GetLastWin32Error() = {err}");
                    if (err == 87) Console.WriteLine("ERROR_INVALID_PARAMETER (87) — thường là struct size/layout không đúng hoặc build sai x86/x64.");
                    return false;
                }

                Thread.Sleep(10);
            }
            return true;
        }

        public static void ForceForeground(IntPtr hWnd)
        {
            ShowWindow(hWnd, SW_SHOW);
            BringWindowToTop(hWnd);

            uint targetThread = GetWindowThread(hWnd);
            uint thisThread = GetCurrentThreadId();
            bool attached = false;
            if (targetThread != thisThread)
            {
                attached = AttachThreadInput(thisThread, targetThread, true);
                Console.WriteLine("AttachThreadInput: " + attached);
            }

            bool setfg = SetForegroundWindow(hWnd);
            Console.WriteLine("SetForegroundWindow: " + setfg);

            if (attached)
            {
                AttachThreadInput(thisThread, targetThread, false);
            }
        }

        public static uint GetWindowThread(IntPtr hWnd)
        {
            uint pid;
            return GetWindowThreadProcessId(hWnd, out pid);
        }
    }
}
