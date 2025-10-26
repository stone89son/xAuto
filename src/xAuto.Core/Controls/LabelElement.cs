using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using xAuto.Core.Helpers;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a Label or Text control (read-only text).
    /// </summary>
    public class LabelElement : UIElementBase
    {
        public LabelElement(AutomationElement element) : base(element) {
#if DEBUG
            DebugHelper.Highlight(element);
#endif
        }

        /// <summary>
        /// Get the displayed text of the label.
        /// </summary>
        public string GetText()
        {
            return Element.Current.Name;
        }
    }
}
