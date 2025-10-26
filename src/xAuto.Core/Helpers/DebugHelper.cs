using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using static xAuto.Core.Sys;
using Point = System.Drawing.Point;
using xAuto.Overlay;
namespace xAuto.Core.Helpers
{
    public class DebugHelper
    {
        public static void DumpAll()
        {
            DumpElementLoop(AutomationElement.RootElement, "");
        }

        private static void DumpElementLoop(AutomationElement element, string xpath, int indent = 0)
        {
            if (element == null) return;

            string indentStr = new string(' ', indent * 2);

            string name = Utils.EscapeQuotes(element.Current.Name);
            string controlType = element.Current.ControlType?.ProgrammaticName ?? "";
            string className = element.Current.ClassName ?? "";
            string automationId = element.Current.AutomationId ?? "";
            string localizedType = element.Current.LocalizedControlType ?? "";

            string typeName = controlType.Replace("ControlType.", "");

            // XPath đơn giản
            string thisXPath = $"{xpath}{typeName}";
            if (!string.IsNullOrEmpty(name))
                thisXPath += $"[@Name='{name}']";
            if (!string.IsNullOrEmpty(automationId))
                thisXPath += $"[@AutomationId='{automationId}']";

            Console.WriteLine($"{indentStr}- [{typeName}] \"{name}\" (Class=\"{className}\", Id=\"{automationId}\", Type=\"{localizedType}\")");
            Console.WriteLine($"{indentStr}  XPath: {thisXPath}");

            // Lấy tất cả phần tử con
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement child = walker.GetFirstChild(element);
            while (child != null)
            {
                DumpElementLoop(child, thisXPath + "/", indent + 1);
                child = walker.GetNextSibling(child);
            }
        }

        public static void Highlight(AutomationElement element)
        {
            if (element == null) return;

            var rect = element.Current.BoundingRectangle;

            if (rect.IsEmpty) return;

            AutomationElement textChild = element.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));

            if (textChild != null)
            {
                rect = (System.Windows.Rect)textChild.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            }

            // Tạo overlay Form
            //var form = new fHover(new System.Drawing.Point(10, 10), 100, 50);
            var form =new xAuto.Overlay.fHover(new Point((int)rect.X, (int)rect.Y), (int)rect.Width, (int)rect.Height);
            form.Show();
            //To render border
            Application.DoEvents();
            Sleep(Config.SpeedDown);
            form.Close();

            Application.DoEvents();
            Sleep(Config.SpeedDown);
        }

        public static void HighlightPoint(int x, int y)
        {
            // Tạo overlay Form
            var form = new xAuto.Overlay.fHover(new Point((int)x, (int)y), (int)5, (int)5);
            form.Show();
            //To render border
            Application.DoEvents();
            Sleep(Config.SpeedDown);
            form.Close();
            Application.DoEvents();
            Sleep(Config.SpeedDown);
        }
    }
}
