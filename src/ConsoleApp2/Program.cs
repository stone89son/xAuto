// Requires references: UIAutomationClient, UIAutomationTypes
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using xAuto.Core.Helpers;

class Program
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetCursorPos(int X, int Y);

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public uint type;
        public MOUSEKEYBDHARDWAREINPUT mkhi;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
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

    const uint INPUT_MOUSE = 0;
    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    static void Main()
    {
        Console.WriteLine("Press any key to start");
        Console.ReadLine();
        Thread.Sleep(5000);
        Console.WriteLine("Starting...");
        string text = "moyai25";
        // 1) Mở On-Screen Keyboard (osk.exe). Nếu bạn dùng touch keyboard (TabTip) có thể mở TabTip.exe thay.

        //var path64 = Path.Combine(Directory.GetDirectories(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "winsxs"), "amd64_microsoft-windows-osk_*")[0], "osk.exe");
        //var path32 = @"C:\windows\system32\osk.exe";
        //var path = (Environment.Is64BitOperatingSystem) ? path64 : path32;

        ProcessHelper.Kill("TabTip");
        Process p = System.Diagnostics.Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");

        // var p = Process.Start(@"C:\Windows\Sysnative\osk.exe");
        if (p == null)
        {
            Console.WriteLine("Không mở được osk.exe");
            return;
        }

        // 2) Chờ OSK hiện lên
        Thread.Sleep(800);

        // 3) Lặp từng ký tự, tìm nút OSK tương ứng và click vào vị trí giữa nút
        foreach (char ch in text)
        {
            bool clicked = ClickOskKey(ch, 5000); // timeout 5000ms
            Console.WriteLine($"Char '{ch}' -> clicked: {clicked}");
            Thread.Sleep(80); // chờ giữa phím; tăng nếu control cần chậm hơn
        }

        Console.WriteLine("Xong. Nhấn Enter để kết thúc.");
        Console.ReadLine();
    }

    static bool ClickOskKey(char ch, int timeoutMs)
    {
        // OSK hiển thị ký tự theo mặt phím — tên nút thường là chính ký tự (ví dụ "m", "o", "y"...) 
        // nhưng có thể là khác (ví dụ "Space", "Enter"). Ta thử match NameProperty == ký tự hoặc ToUpper/ToLower.
        var root = AutomationElement.RootElement;
        var watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            // Tìm cửa sổ OSK bằng title phổ biến. Nếu OSK tiếng Việt/locale khác, bạn có thể tìm bằng ProcessId của osk.exe.
            var oskWindow = root.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "キーボード グリッド"));

            if (oskWindow == null)
            {
                // try find by process name "osk"
                var all = root.FindAll(TreeScope.Children, Condition.TrueCondition);
                foreach (AutomationElement a in all)
                {
                    try
                    {
                        var pid = a.Current.ProcessId;
                        var proc = Process.GetProcessById(pid);
                        if (proc.ProcessName.ToLower().Contains("TabTip"))
                        {
                            oskWindow = a;
                            break;
                        }
                    }
                    catch { }
                }
            }

            if (oskWindow == null)
            {
                Thread.Sleep(200);
                continue;
            }

            // Tìm nút button trong OSK có NameProperty tương ứng ký tự
            // thử 3 biến thể: chính ký tự, ToUpper, ToLower
            string[] candidates = new string[] { ch.ToString(), char.ToUpper(ch).ToString(), char.ToLower(ch).ToString() };

            AutomationElement keyButton = null;
            foreach (var name in candidates)
            {
                keyButton = oskWindow.FindFirst(TreeScope.Descendants,
                    new AndCondition(
                        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                        new PropertyCondition(AutomationElement.NameProperty, name)
                    ));
                if (keyButton != null) break;
            }

            // Nếu không tìm theo Name, thử match AutomationId hoặc help text / partial name
            if (keyButton == null)
            {
                // tìm button chứa ký tự trong Name
                var allButtons = oskWindow.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
                for (int i = 0; i < allButtons.Count; i++)
                {
                    try
                    {
                        var name = allButtons[i].Current.Name;
                        if (!string.IsNullOrEmpty(name) && name.IndexOf(ch.ToString(), StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            keyButton = allButtons[i];
                            break;
                        }
                    }
                    catch { }
                }
            }

            if (keyButton == null)
            {
                // không tìm thấy phím
                return false;
            }

            // Lấy bounding rectangle của nút
            var rect = keyButton.Current.BoundingRectangle;
            if (rect.IsEmpty)
            {
                return false;
            }

            int cx = (int)((rect.Left + rect.Right) / 2);
            int cy = (int)((rect.Top + rect.Bottom) / 2);

            // Di chuyển chuột tới đó và click
            SetCursorPos(cx, cy);
            Thread.Sleep(20);
            MouseLeftClick();
            return true;
        }

        return false;
    }

    static void MouseLeftClick()
    {
        INPUT down = new INPUT();
        down.type = INPUT_MOUSE;
        down.mkhi.mi = new MOUSEINPUT { dx = 0, dy = 0, mouseData = 0, dwFlags = MOUSEEVENTF_LEFTDOWN, time = 0, dwExtraInfo = IntPtr.Zero };

        INPUT up = new INPUT();
        up.type = INPUT_MOUSE;
        up.mkhi.mi = new MOUSEINPUT { dx = 0, dy = 0, mouseData = 0, dwFlags = MOUSEEVENTF_LEFTUP, time = 0, dwExtraInfo = IntPtr.Zero };

        INPUT[] inputs = new INPUT[] { down, up };
        var res = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        if (res == 0)
        {
            int err = Marshal.GetLastWin32Error();
            Console.WriteLine("SendInput(mouse) failed: " + err);
        }
    }
}
