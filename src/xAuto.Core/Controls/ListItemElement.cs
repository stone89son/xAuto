using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a ListItem control (e.g., items inside lists or combo boxes).
    /// </summary>
    public class ListItemElement : UIElementBase
    {
        public ListItemElement(AutomationElement element) : base(element) { }

        public void Select()
        {
            if (Element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
            {
                ((SelectionItemPattern)pattern).Select();
            }
            else
            {
                throw new InvalidOperationException("ListItem does not support SelectionItemPattern.");
            }
        }

        public bool IsSelected()
        {
            if (Element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
            {
                return ((SelectionItemPattern)pattern).Current.IsSelected;
            }
            return false;
        }
    }
}
