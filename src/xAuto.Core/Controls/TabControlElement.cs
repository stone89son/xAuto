using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a TabControl container.
    /// </summary>
    public class TabControlElement : UIElementBase
    {
        public TabControlElement(AutomationElement element) : base(element) { }

        /// <summary>
        /// Get all TabItem elements within this TabControl.
        /// </summary>
        public List<TabItemElement> GetTabs()
        {
            var tabs = new List<TabItemElement>();
            var children = Element.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                if (child.Current.ControlType == ControlType.TabItem)
                {
                    tabs.Add(new TabItemElement(child));
                }
            }
            return tabs;
        }

        /// <summary>
        /// Select a tab by its name.
        /// </summary>
        public void SelectTabByName(string tabName)
        {
            var tab = Element.FindFirst(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.NameProperty, tabName));

            if (tab != null)
            {
                var tabItem = new TabItemElement(tab);
                tabItem.Select();
            }
            else
            {
                throw new InvalidOperationException($"Tab '{tabName}' not found.");
            }
        }
    }
}
