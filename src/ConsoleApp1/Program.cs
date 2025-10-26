using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
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
            public InputUnion U;
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
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

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

        static void Main()
        {
            string windowTitle = "無題 - メモ帳"; // đổi theo target, thử Notepad trước
            string textToSend = "moyai25";

            var hWnd = FindWindow(null, windowTitle);
            if (hWnd == IntPtr.Zero)
            {
                Console.WriteLine("Không tìm thấy cửa sổ: " + windowTitle);
                return;
            }
            Console.WriteLine("Found hWnd: " + hWnd);

            // Cố gắng đặt foreground (AttachThreadInput trick)
            ForceForeground(hWnd);
            Thread.Sleep(120);

            bool ok = SendUnicodeText(textToSend);
            Console.WriteLine("SendText result: " + (ok ? "OK" : "FAILED"));
            Console.WriteLine("Nhấn Enter để thoát.");
            Console.ReadLine();
        }

        static void ForceForeground(IntPtr hWnd)
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

        static uint GetWindowThread(IntPtr hWnd)
        {
            uint pid;
            return GetWindowThreadProcessId(hWnd, out pid);
        }

        static bool SendUnicodeText(string text)
        {
            int size = Marshal.SizeOf(typeof(INPUT));
            Console.WriteLine("Marshal.SizeOf(INPUT) = " + size);

            foreach (char ch in text)
            {
                INPUT down = new INPUT();
                down.type = INPUT_KEYBOARD;
                down.U = new InputUnion();
                down.U.ki = new KEYBDINPUT
                {
                    wVk = 0,
                    wScan = (ushort)ch,
                    dwFlags = KEYEVENTF_UNICODE,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                };

                INPUT up = new INPUT();
                up.type = INPUT_KEYBOARD;
                up.U = new InputUnion();
                up.U.ki = new KEYBDINPUT
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

                Thread.Sleep(20);
            }
            return true;
        }
    }

}
