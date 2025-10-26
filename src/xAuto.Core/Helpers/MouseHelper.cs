using AutoIt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core.Helpers
{
    public class MouseHelper
    {
        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static void LockMouseAt(int x, int y)
        {
            RECT rect = new RECT { Left = x, Top = y, Right = x + 1, Bottom = y + 1 };
            ClipCursor(ref rect);
        }

        public static void UnlockMouse()
        {
            ClipCursor(IntPtr.Zero);
        }


        public static void LeftClick(int x, int y)
        {
#if DEBUG
            DebugHelper.HighlightPoint(x, y);
#endif
            LockMouseAt(x, y);
            AutoItX.MouseClick("left", x, y);
            UnlockMouse();
        }
    }
}
