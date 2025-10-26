using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using xAuto.Core.Helpers;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a RadioButton control.
    /// </summary>
    public class RadioButtonElement : UIElementBase
    {
        public RadioButtonElement(AutomationElement element) : base(element)
        {
#if DEBUG
            DebugHelper.Highlight(element);
#endif
        }

        override
        public void Click()
        {
            if (Element.TryGetCurrentPattern(InvokePattern.Pattern, out var invoke))
            {
                var inv = (InvokePattern)invoke;
                inv.Invoke();
            }
            MouseHelper.LeftClick((int)Element.Current.BoundingRectangle.X+30, (int)Element.Current.BoundingRectangle.Y + 5);
        }
        public void Select()
        {
            var pattern = Element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
            if (pattern == null)
                throw new InvalidOperationException("RadioButton does not support SelectionItemPattern.");

            pattern.Select();
        }

        public bool IsSelected()
        {
            var pattern = Element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
            return pattern?.Current.IsSelected ?? false;
        }
    }
}
