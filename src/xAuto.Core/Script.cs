using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core
{
    public class Script
    {
        public Setting settings { get; set; } = new Setting();
        public string AppPath { get; set; }
        public List<Window> Windows { get; set; }
    }

    public class Window
    {
        public string Name { get; set; }
        public ScriptStep[] Steps { get; set; }
    }
}
