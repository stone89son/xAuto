using AutoIt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using xAuto.Core.Helpers;

namespace xAuto.Core.Controls
{
    /// <summary>
    /// Represents a TextBox control.
    /// </summary>
    public class TextBoxElement : UIElementBase
    {
        public TextBoxElement(AutomationElement element) : base(element)
        {
#if DEBUG
          DebugHelper.Highlight(element);
#endif
        }

        public void SetText(string text)
        {
           
            //Support for ValuePattern
            var patterns = Element.GetSupportedPatterns();
            Console.WriteLine("Supported Patterns:");
            foreach (var p in patterns)
            {
                Console.WriteLine(p.ProgrammaticName);
            }
            if (Element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
            {
                ((ValuePattern)pattern).SetValue(text);
            }
            else
            {
                // throw new InvalidOperationException("TextBox does not support ValuePattern.");
            }
        }

        public string GetText()
        {
            if (Element.TryGetCurrentPattern(ValuePattern.Pattern, out object pattern))
            {
                return ((ValuePattern)pattern).Current.Value;
            }
            return string.Empty;
        }
    }
}
