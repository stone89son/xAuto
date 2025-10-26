using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xAuto.Core.Enums;

namespace xAuto.Core
{
    public class ScriptStep
    {
        // JSON sẽ truyền chuỗi, converter sẽ parse thành enum
        [JsonConverter(typeof(StringEnumConverter))]
        public ActionType action { get; set; }

        public string target { get; set; }
        public string value { get; set; }
        public string controlType { get; set; }
        public int? timeout { get; set; }  // optional, override globalTimeout
    }
}
