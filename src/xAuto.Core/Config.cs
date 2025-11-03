using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public static class Config
    {
        public static double SpeedDown = 0.01;
        public static double AppOpenTimeout { get; set; } = 60; //60s
        public static double AppInstallTimeout { get; set; } =5*60;//60s
        public static double FindControlTimeout { get; set; } = 120; //120s
        public static double PollInterval { get; set; } = 0.1; //0.2s
    }
}
