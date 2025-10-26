using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a ComboBox control.
    /// </summary>
    public class ComboBoxElement : UIElementBase
    {
        public ComboBoxElement(AutomationElement element) : base(element) { }

        public void Expand()
        {
            var expandPattern = Element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            expandPattern?.Expand();
        }

        public void Collapse()
        {
            var expandPattern = Element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
            expandPattern?.Collapse();
        }

        public void SelectItem(string itemName)
        {
            Expand();
            var item = Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, itemName));
            if (item != null && item.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
            {
                ((SelectionItemPattern)pattern).Select();
                Collapse();
            }
            else
            {
                throw new InvalidOperationException($"Item '{itemName}' not found in ComboBox.");
            }
        }
    }
}
