using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Xml.Linq;
using xAuto.Core.Helpers;

namespace xAuto.Core.UIElement
{
    public class UIElementFinder
    {
      
        public static AutomationElement GetWindowDeep(string windowPartName)
        {
            return GetWindowLoop(AutomationElement.RootElement, windowPartName);
        }

        public static AutomationElement GetElement(AutomationElement parentElement, string windowPartName)
        {
            return GetWindowLoop(parentElement, windowPartName);
        }

        public static AutomationElement GetWindowLoop(AutomationElement element, string windowPartName, int indent = 0)
        {
            if (element == null) return null;

            try
            {
                var children = element.FindAll(TreeScope.Children, Condition.TrueCondition);
                foreach (AutomationElement child in children)
                {
                    if (IsElementAvailable(child)
                        && !string.IsNullOrWhiteSpace(child.Current.Name)
                        && child.Current.Name.Contains(windowPartName))
                    {
                        return child;
                    }

                    GetWindowLoop(child, windowPartName, indent + 1);
                }
            }
            catch (ElementNotAvailableException)
            {
                // Some UI elements disappear during traversal — skip them
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Some “NonClientArea” regions cause COM errors — skip them
            }
            return null;
        }

        public static AutomationElement DeepFindElement(string windowNamePart, string elementNamePart)
        {
            AutomationElement mainWindow = GetWindowByNameContains(windowNamePart);
            if (mainWindow == null)
            {
                return null;
            }

            Dictionary<string, AutomationElement> allControls = new Dictionary<string, AutomationElement>();
            CollectAllControls(mainWindow, "", allControls);
            foreach (var kvp in allControls)
            {
                string path = kvp.Key;
                AutomationElement element = kvp.Value;

                if (path.IndexOf(elementNamePart, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    //#if DEBUG        
                    //element.Current.
                    //Logger.WriteLine($"Found: {path} - Type: {element.Current.ControlType.ProgrammaticName} {element.Current.Name}");
                    //#endif
                    return element;

                }
            }
            return null;
        }

        private static void CollectAllControls(AutomationElement parent, string parentPath, Dictionary<string, AutomationElement> result)
        {
            AutomationElementCollection children = parent.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement child in children)
            {
                try
                {
                    if (IsElementAvailable(child))
                    {
                        string name = string.IsNullOrEmpty(child.Current.Name) ? "(no name)" : child.Current.Name;
                        string type = child.Current.ControlType.ProgrammaticName;
                        string fullPath = string.IsNullOrEmpty(parentPath) ? name : parentPath + "\\" + name;

                        // Nếu trùng tên, thêm số để tránh mất control
                        int suffix = 1;
                        string uniquePath = fullPath;
                        while (result.ContainsKey(uniquePath))
                        {
                            suffix++;
                            uniquePath = fullPath + $"[{suffix}]";
                        }

                        result.Add(uniquePath, child);
                        CollectAllControls(child, uniquePath, result);
                    }
                }
                catch (ElementNotAvailableException) { }
                catch (COMException) { }
            }
        }

        /// <summary>
        /// Wait until a window with name containing 'windowNamePart' appears, or until timeout (ms)
        /// </summary>
        public static AutomationElement WaitForWindowByNameContains(string windowNamePart, int timeoutMs = 30000, int pollInterval = 500)
        {
            if (string.IsNullOrEmpty(windowNamePart))
                return null;

            AutomationElement window = null;
            int waited = 0;

            while (waited < timeoutMs)
            {
                window = GetWindowByNameContains(windowNamePart);
                if (window != null)
                    break;

                Thread.Sleep(pollInterval);
                waited += pollInterval;
            }

            return window; // null if not found
        }

        /// <summary>
        /// Get window by contains name (title)
        /// </summary>
        public static AutomationElement GetWindowByNameContains(string windowPartName)
        {
            var windows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);

            foreach (AutomationElement window in windows)
            {
                if (IsElementAvailable(window)
                    && window.Current.Name != null
                    && window.Current.Name.Contains(windowPartName))
                {
                    return window;
                }
            }

            return null;
        }

        /// <summary>
        /// Get windows by contains name (title)
        /// </summary>
        public static List<AutomationElement> GetWindowsByNameContains(string windowPartName)
        {
            List<AutomationElement> windowFounds = new List<AutomationElement>();
            var windows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);


            foreach (AutomationElement window in windows)
            {
                if (IsElementAvailable(window)
                    && window.Current.Name != null
                    && window.Current.Name.Contains(windowPartName))
                {
                    windowFounds.Add(window);
                }
            }

            return windowFounds;
        }

        /// <summary>
        /// Get window by name (title)
        /// </summary>
        public static AutomationElement GetWindowByName(string windowName)
        {
            return AutomationElement.RootElement.FindFirst(
                TreeScope.Children,
                new PropertyCondition(AutomationElement.NameProperty, windowName)
            );
        }

        public static AutomationElement FindElementFlexible(AutomationElement parent,
            string namePart = null,
            string automationId = null,
            string className = null,
            ControlType controlType = null,
            bool partialMatch=true)
        {
            if (parent == null)
                return null;

            var allElements = parent.FindAll(TreeScope.Descendants, Condition.TrueCondition);

            foreach (AutomationElement element in allElements)
            {
                if (!IsElementAvailable(element))
                    continue;

                bool match = true;

                if (!string.IsNullOrEmpty(namePart))
                {
                    string name = element.Current.Name ?? "";
                    if (partialMatch)
                    {
                        if (!name.Contains(namePart))
                            match = false;
                    }
                    else
                    {
                        if (!name.Equals(namePart, StringComparison.OrdinalIgnoreCase))
                            match = false;
                    }
                }

                if (!string.IsNullOrEmpty(automationId))
                {
                    string aid = element.Current.AutomationId ?? "";
                    if (!aid.Equals(automationId, StringComparison.OrdinalIgnoreCase))
                        match = false;
                }

                if (!string.IsNullOrEmpty(className))
                {
                    string cls = element.Current.ClassName ?? "";
                    if (!cls.Equals(className, StringComparison.OrdinalIgnoreCase))
                        match = false;
                }

                if (controlType != null && element.Current.ControlType != controlType)
                    match = false;

                if (match)
                    return element;
            }

            return null;
        }

        public static AutomationElement FindByXPath(AutomationElement window, string xpath)
        {
            if (window == null || string.IsNullOrWhiteSpace(xpath))
                return null;

            // Chuẩn hóa: bỏ dấu "/" đầu
            xpath = xpath.Trim();
            if (xpath.StartsWith("//"))
                xpath = xpath.Substring(2);

            // Tách các bước (ví dụ: Pane[@AutomationId='main']//Button[2])
            string[] steps = xpath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            AutomationElement current = window;

            foreach (string rawStep in steps)
            {
                if (current == null) return null;

                string step = rawStep.Trim();

                // Bắt ControlType
                var matchType = Regex.Match(step, @"^(\w+)");
                string controlType = matchType.Success ? matchType.Groups[1].Value : null;

                // Bắt điều kiện [@Name='xx'] hoặc [@AutomationId='xx']
                var conditions = new Dictionary<string, string>();
                foreach (Match m in Regex.Matches(step, @"@(\w+)\s*=\s*'([^']*)'"))
                    conditions[m.Groups[1].Value] = m.Groups[2].Value;

                // Bắt chỉ số [n]
                int? index = null;
                var matchIndex = Regex.Match(step, @"\[(\d+)\]");
                if (matchIndex.Success)
                    index = int.Parse(matchIndex.Groups[1].Value);

                // Tìm tất cả phần tử con
                var walker = TreeWalker.ControlViewWalker;
                var children = new List<AutomationElement>();
                AutomationElement child = walker.GetFirstChild(current);
                while (child != null)
                {
                    if (IsElementAvailable(child))
                        children.Add(child);
                    child = walker.GetNextSibling(child);
                }

                // Lọc theo ControlType
                var filtered = new List<AutomationElement>();
                foreach (var el in children)
                {
                    if (controlType != null &&
                        !el.Current.ControlType.ProgrammaticName.EndsWith(controlType, StringComparison.OrdinalIgnoreCase))
                        continue;

                    bool match = true;
                    foreach (var kv in conditions)
                    {
                        string actual = null;
                        if (kv.Key.Equals("Name", StringComparison.OrdinalIgnoreCase))
                            actual = el.Current.Name;
                        else if (kv.Key.Equals("AutomationId", StringComparison.OrdinalIgnoreCase))
                            actual = el.Current.AutomationId;
                        else if (kv.Key.Equals("ClassName", StringComparison.OrdinalIgnoreCase))
                            actual = el.Current.ClassName;

                        if (!string.Equals(actual, kv.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                        filtered.Add(el);
                }

                // Chọn phần tử theo index (nếu có)
                if (filtered.Count == 0)
                    return null;
                if (index.HasValue)
                {
                    int idx = Math.Min(index.Value, filtered.Count);
                    current = filtered[idx - 1]; // XPath index bắt đầu từ 1
                }
                else
                    current = filtered[0];
            }

            return current;
        }

        private static bool IsElementAvailable(AutomationElement element)
        {
            try
            {
                var _ = element.Current.Name;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
