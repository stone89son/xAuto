using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a MenuItem control (used in application menus).
    /// </summary>
    public class MenuItemElement : UIElementBase
    {
        public MenuItemElement(AutomationElement element) : base(element) { }

        /// <summary>
        /// Expand submenu if it has children.
        /// </summary>
        public void Expand()
        {
            if (Element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object pattern))
            {
                ((ExpandCollapsePattern)pattern).Expand();
            }
        }

        /// <summary>
        /// Collapse submenu.
        /// </summary>
        public void Collapse()
        {
            if (Element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object pattern))
            {
                ((ExpandCollapsePattern)pattern).Collapse();
            }
        }

        /// <summary>
        /// Click or invoke the menu item.
        /// </summary>
        public override void Click()
        {
            base.Click();
        }
    }
}
