using AutoIt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using WindowsInput;
using xAuto.Core.Helpers;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a TreeItem node inside a TreeView.
    /// </summary>
    public class TreeItemElement : UIElementBase
    {
        public TreeItemElement(AutomationElement element) : base(element) {
#if DEBUG
            DebugHelper.Highlight(element);
#endif
        }

        public void Expand()
        {
            if (Element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object pattern))
            {
                ((ExpandCollapsePattern)pattern).Expand();
            }
        }

        public void Collapse()
        {
            if (Element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out object pattern))
            {
                ((ExpandCollapsePattern)pattern).Collapse();
            }
        }

        public void Select()
        {
            if (Element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out object pattern))
            {
                ((SelectionItemPattern)pattern).Select();
            }
        }

        /// <summary>
        /// Set focus vào TreeItem 
        /// </summary>
        public void SetFocus()
        {
            try
            {
                Element.SetFocus();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Không thể focus TreeItem này (InvalidOperationException).");
            }
            catch (ElementNotAvailableException)
            {
                Console.WriteLine("Element không khả dụng (ElementNotAvailableException).");
            }
        }

        public void LeftClick()
        {
            try
            {
                var rect = Element.Current.BoundingRectangle;
                if (rect.IsEmpty)
                {
                    Console.WriteLine("Không lấy được toạ độ của TreeItem.");
                    return;
                }
               
                int clickX = (int)(rect.Left - 24);
                int clickY = (int)(rect.Top + rect.Height / 2);
                
                Thread.Sleep(100);
                MouseHelper.LeftClick(clickX, clickY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi RightClick(): {ex.Message}");
            }
        }


        public List<TreeItemElement> GetChildren()
        {
            var result = new List<TreeItemElement>();
            var children = Element.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                if (child.Current.ControlType == ControlType.TreeItem)
                {
                    result.Add(new TreeItemElement(child));
                }
            }
            return result;
        }
    }
}
