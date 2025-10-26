using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core
{
    /// <summary>
    /// Base class for all UI elements. 
    /// Provides common methods for locating and interacting with AutomationElements.
    /// </summary>
    public abstract class UIElementBase
    {
        protected AutomationElement Element;

        protected UIElementBase(AutomationElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Check if the element is available and enabled.
        /// </summary>
        public bool IsEnabled => Element.Current.IsEnabled;

        /// <summary>
        /// Get element name.
        /// </summary>
        public string Name => Element.Current.Name;

        /// <summary>
        /// Click using InvokePattern if supported.
        /// </summary>
        public virtual void Click()
        {
            if (Element.TryGetCurrentPattern(InvokePattern.Pattern, out object pattern))
            {
                ((InvokePattern)pattern).Invoke();

            }
            else
            {
                throw new InvalidOperationException("Element does not support InvokePattern.");
            }
        }

        /// <summary>
        /// Find a child element by name.
        /// </summary>
        public AutomationElement FindChildByName(string name)
        {
            return Element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, name));
        }
    }
}
