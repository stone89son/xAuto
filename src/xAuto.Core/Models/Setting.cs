using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public class Setting
    {
        public int GlobalTimeout { get; set; } = 30000; //30s
        public int PollInterval { get; set; } = 500; //5s
    }
}
