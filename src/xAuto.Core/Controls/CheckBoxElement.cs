using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a CheckBox control.
    /// </summary>
    public class CheckBoxElement : UIElementBase
    {
        public CheckBoxElement(AutomationElement element) : base(element) { }

        public void SetChecked(bool isChecked)
        {
            var togglePattern = Element.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
            if (togglePattern == null)
                throw new InvalidOperationException("CheckBox does not support TogglePattern.");

            bool currentState = togglePattern.Current.ToggleState == ToggleState.On;
            if (currentState != isChecked)
            {
                togglePattern.Toggle();
            }
        }

        public bool IsChecked()
        {
            var togglePattern = Element.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
            return togglePattern?.Current.ToggleState == ToggleState.On;
        }
    }
}
