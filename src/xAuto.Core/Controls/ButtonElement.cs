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
    /// Represents a Button control.
    /// </summary>
    public class ButtonElement : UIElementBase
    {
        public ButtonElement(AutomationElement element) : base(element) {
#if DEBUG
            DebugHelper.Highlight(element);
#endif
        }

        public override void Click()
        {
            base.Click();
        }
    }
}
