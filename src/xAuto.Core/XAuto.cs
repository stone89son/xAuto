using AutoIt;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml.Linq;
using xAuto.Core.Controls;
using xAuto.Core.Helpers;
using xAuto.Core.UIElement;
using static System.Net.Mime.MediaTypeNames;

namespace xAuto.Core
{
    public class XAuto
    {
        public static T FindControl<T>(AutomationElement window, string namePart) where T : UIElementBase
        {
            return UIElementFactory.FindControl<T>(window, namePart);
        }

        public static AutomationElement FindWindow(AutomationElement element, string namePartWindow)
        {
            if (element == null) return null;
            string name = Utils.EscapeQuotes(element.Current.Name);
            string controlType = element.Current.ControlType?.ProgrammaticName ?? "";
            string className = element.Current.ClassName ?? "";
            string automationId = element.Current.AutomationId ?? "";
            string localizedType = element.Current.LocalizedControlType ?? "";

            string typeName = controlType.Replace("ControlType.", "");
            if (name.Contains(namePartWindow))
            {
                return element;
            }

            // Get all child elements
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement child = walker.GetFirstChild(element);
            while (child != null)
            {
                FindWindow(child, namePartWindow);
                child = walker.GetNextSibling(child);
            }
            return null;
        }

        public static AutomationElement GetWindow(string namePartWindow)
        {
            AutomationElement window = null;
            Logger.WriteLine($"{new string(' ', 5)}Wait {namePartWindow}");
            bool isClicked = WaitHelper.WaitWindowUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowDeep(namePartWindow);
                    if (window != null)
                    {
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Found window '{namePartWindow}' timeout!.");
            }

            return window;
        }

        public static AutomationElement WaitUtilsExitsWindow(string namePartWindow)
        {
            AutomationElement window = null;
            Logger.WriteLine($"{new string(' ', 5)}Wait {namePartWindow}");
            bool isClicked = WaitHelper.WaitWindowUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowDeep(namePartWindow);
                    if (window != null)
                    {
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Found window '{namePartWindow}' timeout!.");
            }

            return window;
        }

        public static AutomationElement Click<T>(string namePartWindow, string elementNamePart) where T : UIElementBase
        {
            AutomationElement window = null;
            Logger.WriteLine($"{new string(' ', 5)}Click {elementNamePart}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowByNameContains(namePartWindow);
                    var control = UIElementFactory.FindControl<T>(window, elementNamePart);
                    if (control != null)
                    {
                        control.Click();
                        //switch (control)
                        //{
                        //    case CheckBoxElement checkbox:
                        //        checkbox.SetChecked(isCheckValue);
                        //        break;
                        //    case ButtonElement button:
                        //        button.Click();
                        //        break;
                        //    default:
                        //        control.Click(); // fallback cho các control khác
                        //        break;
                        //}
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Click: '{elementNamePart}' timeout!.");
            }
            return window;
        }

        public static AutomationElement SetText<T>(string namePartWindow, string xpath,string value) where T : UIElementBase
        {
            AutomationElement window = null;
            Logger.WriteLine($"{new string(' ', 5)}Click {xpath}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowDeep(namePartWindow);
                    var control = UIElementFactory.FindControlByXpath<T>(window, xpath);
                    var Element =  UIElementFinder.FindByXPath(window, xpath);
                    if (control != null)
                    {
                        switch (control)
                        {
                            case TextBoxElement textBox:

                                AutoItX.WinActivate("TightVNC Server: Set Passwords");
                                Thread.Sleep(50);
                                MouseHelper.LeftClick((int)Element.Current.BoundingRectangle.X + 10, (int)Element.Current.BoundingRectangle.Y + 5);
                              
                                Thread.Sleep(50);
                                AutoItX.Send(value); // Select all existing text
                                Thread.Sleep(2000);
                                // textBox.SetText(value);

                                break;
                        }
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Set text: '{xpath}=\"{value}\"' timeout!.");
            }
            return window;
        }

        public static AutomationElement CheckUntilExists<T>(string namePartWindow,
            string elementNamePart, bool checkValue) where T : UIElementBase
        {
            AutomationElement window = null;
            Logger.WriteLine($"{new string(' ', 5)}Check {elementNamePart}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowByNameContains(namePartWindow);
                    var control = UIElementFactory.FindControl<T>(window, elementNamePart);
                    if (control != null && control is CheckBoxElement checkbox)
                    {
                        checkbox.SetChecked(checkValue);
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Check '{elementNamePart}' timeout!.");
            }
            return window;
        }

        public static void CheckUntilExists<T>(AutomationElement window,
       string elementNamePart, bool checkValue) where T : UIElementBase
        {
            Logger.WriteLine($"{new string(' ', 5)}Check {elementNamePart}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    var control = UIElementFactory.FindControl<T>(window, elementNamePart);
                    if (control != null && control is CheckBoxElement checkbox)
                    {
                        checkbox.SetChecked(checkValue);
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Check: '{elementNamePart}' timeout!.");
            }
        }

        public static void Click<T>(AutomationElement window, string elementNamePart) where T : UIElementBase
        {
            Logger.WriteLine($"{new string(' ', 5)}Click {elementNamePart}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    var control = UIElementFactory.FindControl<T>(window, elementNamePart);
                    if (control != null)
                    {
                        control.Click();
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Click '{elementNamePart}' timeout!.");
            }
        }

        public static void ClickByXpath<T>(AutomationElement window, string xpath) where T : UIElementBase
        {
            Logger.WriteLine($"{new string(' ', 5)}Click {xpath}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    var control = UIElementFactory.FindControlByXpath<T>(window, xpath);
                    if (control != null)
                    {
                        control.Click();
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Click '{xpath}' timeout!.");
            }
        }
        
        public static void DeepClickUntilExists<T>(string windowPartName, string elementPartName) where T : UIElementBase
        {
            Logger.WriteLine($"{new string(' ', 5)}Click {elementPartName}");
            bool isClicked = WaitHelper.WaitUntilTimeout(
                () =>
                {
                    var control = UIElementFactory.DeepFindControl<T>(windowPartName, elementPartName);
                    if (control != null)
                    {
                        switch (control)
                        {
                            case TreeItemElement treeItem:
                                treeItem.Select();
                                treeItem.LeftClick();
                                //treeItem.Expand();
                                break;
                            default:
                                control.Click(); // fallback cho các control khác
                                break;
                        }
                        return true;
                    }
                    return false;
                }
            );
            if (!isClicked)
            {
                throw new Exception($"Click: '{elementPartName}' timeout!.");
            }

        }
        public static AutomationElement WaitUntilExists<T>(string windowNamePart, string elementNamePart) where T : UIElementBase
        {
            Logger.WriteLine($"{new string(' ', 5)}Wait {elementNamePart}");
            AutomationElement window = null;
            bool isClicked = WaitHelper.WaitAppInstallUntilTimeout(
                () =>
                {
                    window = UIElementFinder.GetWindowDeep(windowNamePart);
                    T element = UIElementFactory.FindControl<T>(window, elementNamePart);
                    if (element != null)
                    {
                        return true;
                    }
                    return false;
                }
            );

            if (!isClicked)
            {
                throw new Exception($"Wait: '{elementNamePart}' timeout!.");
            }
            return window;
        }
    }
}
