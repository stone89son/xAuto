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
    /// Represents a TreeView control containing hierarchical TreeItems.
    /// </summary>
    public class TreeViewElement : UIElementBase
    {
        public TreeViewElement(AutomationElement element) : base(element) {
#if DEBUG
            DebugHelper.Highlight(element);
#endif
        }

        /// <summary>
        /// Get all top-level tree items.
        /// </summary>
        public List<TreeItemElement> GetRootItems()
        {
            var items = new List<TreeItemElement>();
            var children = Element.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                if (child.Current.ControlType == ControlType.TreeItem)
                {
                    items.Add(new TreeItemElement(child));
                }
            }
            return items;
        }
    }
}
