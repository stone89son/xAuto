using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public class Logger
    {
        public static void WriteLine(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}
