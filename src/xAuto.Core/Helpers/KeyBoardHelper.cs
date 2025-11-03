using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xAuto.Core.Helpers
{
    public class KeyBoardHelper
    {
        [DllImport("user32.dll")]
        public static extern bool BlockInput(bool block);
    }
}
