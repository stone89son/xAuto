using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using xAuto.Core.Controls;
using xAuto.Core.UIElement;

namespace xAuto.Core
{
    /// <summary>
    /// Factory class that creates the correct UIElement wrapper
    /// based on the control type of the AutomationElement.
    /// </summary>
    public static class UIElementFactory
    {

        /// <summary>
        /// Get control by contains name (title)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="window"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindControl<T>(AutomationElement window, string name) where T : UIElementBase
        {
            AutomationElement automationElement = UIElementFinder.FindElementFlexible(window, namePart: name);
            if (automationElement == null)
                return null;
            else
                return Create<T>(automationElement);
        }

        /// <summary>
        /// Get control by ClassName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="window"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T FindControlByClassName<T>(AutomationElement window, string name) where T : UIElementBase
        {
            AutomationElement automationElement = window.FindFirst(
                 TreeScope.Descendants,
                 new PropertyCondition(AutomationElement.ClassNameProperty, name)
             );

            if (automationElement == null)
                return null;
            else
                return Create<T>(automationElement);
        }

        public static T FindControlByXpath<T>(AutomationElement window, string xpath) where T : UIElementBase
        {
            AutomationElement automationElement = UIElementFinder.FindByXPath(window, xpath);
            if (automationElement == null)
                return null;
            else
                return Create<T>(automationElement);
        }

        /// <summary>
        /// Create the right control wrapper (Button, TextBox, CheckBox, etc.)
        /// </summary>
        /// <param name="element">AutomationElement to wrap</param>
        /// <returns>UIElementBase-derived object or null if not recognized</returns>
        public static T Create<T>(AutomationElement element) where T : UIElementBase
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return (T)Activator.CreateInstance(typeof(T), element);
        }


        public static T DeepFindControl<T>(string windowPartName, string name) where T : UIElementBase
        {
            AutomationElement automationElement = UIElementFinder.DeepFindElement(windowPartName, name);
            if (automationElement == null)
                return null;
            else
                return Create<T>(automationElement);
        }
    }
}
