using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public static class Sys
    {
        public static void Sleep(double seconds) => Thread.Sleep(Convert.ToInt32(seconds * 1000));
    }
}
