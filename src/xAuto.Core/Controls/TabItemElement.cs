using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a TabItem inside a TabControl.
    /// </summary>
    public class TabItemElement : UIElementBase
    {
        public TabItemElement(AutomationElement element) : base(element) { }

        public void Select()
        {
            if (Element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
            {
                ((SelectionItemPattern)pattern).Select();
            }
            else
            {
                throw new InvalidOperationException("TabItem does not support SelectionItemPattern.");
            }
        }
    }
}
