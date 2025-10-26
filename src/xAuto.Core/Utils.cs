using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public class Utils
    {
        public static string EscapeQuotes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Replace("'", "&apos;").Replace("\"", "&quot;");
        }
    }
}
