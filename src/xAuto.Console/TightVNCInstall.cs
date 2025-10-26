using AutoIt;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Xml.Linq;
using WindowsInput;
using xAuto.Core;
using xAuto.Core.Controls;
using xAuto.Core.Helpers;
using xAuto.Core.UIElement;
using static xAuto.Core.Sys;
namespace xAuto
{
    public class TightVNCInstall
    {
        private static string _tightvncPath = @"D:\Projects\xAuto\setup\tightvnc-2.8.85-gpl-setup-64bit.msi";
        public static bool Run()
        {
            RegistryHelper.DeleteRegistryKey(@"HKEY_LOCAL_MACHINE\SOFTWARE\TightVNC");

            Logger.WriteLine("[START] Close TightVNC window that was previously open");
            ProcessHelper.KillForMsi("TightVNC");
            ProcessHelper.Kill("tvnserver");
            Logger.WriteLine("[END] Close TightVNC window that was previously open");
            return Install();
        }

        public static bool Install()
        {
            try
            {
                Process.Start(_tightvncPath);
                Sleep(2);
                Logger.WriteLine("[OPEN] TightVNC Setup");

                var window = XAuto.WaitUtilsExitsWindow("TightVNC Setup");
                XAuto.Click<ButtonElement>(window, "Next");
                window = XAuto.WaitUtilsExitsWindow("TightVNC Setup");
                LabelElement removeLabel = XAuto.FindControl<LabelElement>(window, "remove installation");

                if (removeLabel != null)
                {

                    bool isUnstall = Uninstall();
                    if (!isUnstall)
                    {
                        return false;
                    }
                    Logger.WriteLine("[OPEN] TightVNC setup");
                    Process.Start(_tightvncPath);
                    Sleep(2);
                    window = XAuto.WaitUtilsExitsWindow("TightVNC Setup");
                    XAuto.Click<ButtonElement>(window, "Next");
                }

                Logger.WriteLine("[START] install TightVNC");
                window = XAuto.CheckUntilExists<CheckBoxElement>("TightVNC Setup", "I accept the terms", true);
                XAuto.Click<ButtonElement>("TightVNC Setup", "Next");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Custom");

                XAuto.DeepClickUntilExists<TreeItemElement>("TightVNC Setup", "TightVNC Viewer");
                KeyBoardHelper.BlockInput(true);
                AutoItX.Send("{UP}");
                AutoItX.Send("{ENTER}");
                KeyBoardHelper.BlockInput(false);
                XAuto.Click<ButtonElement>("TightVNC Setup", "Next");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Next");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Install");

                //window = XAuto.CheckUntilExists<CheckBoxElement>("TightVNC Setup", "I accept the terms", true);
                Sleep(2);
                //UIElementFinder.TraverseElemen
                //t(AutomationElement.RootElement, 0, "/");
                //XAuto.WaitUntilExists<LabelElement>("TightVNC Setup", "Completing the TightVNC Setup Wizard", 300);
                //TODO fix slow
                var windowPass = XAuto.WaitUtilsExitsWindow("Set Passwords");

                if (windowPass == null)
                {
                    Logger.WriteLine("Cannot find window Set Passwords");
                    return false;
                }

                IntPtr main = XAutoNative.FindWindow(null, "TightVNC Server: Set Passwords");
                IntPtr main2 = XAutoNative.FindWindow(null, "TightVNC Setup");
                IntPtr edit1 = XAutoNative.FindWindowEx(main, IntPtr.Zero, "Edit", null);
                XAutoNative.SetForegroundWindow(edit1);
                ProcessHelper.RunPassFill();
                XAuto.Click<ButtonElement>("TightVNC Setup", "Finish");
                //var (result, output, error) = ProcessHelper.RunCommand(@"PsExec64.exe -i -s "C:\Users\ThePC\Downloads\AutoHotkey_2.0.19\type.exe"");
                //if (!result)
                //{ 
                //    Logger.WriteLine($"Run type.exe output: {output}");
                //    Logger.WriteLine($"Run type.exe failed: {error}");
                //}

                //AutoItX.Send("moyai25");
                //AutoItX.Send("{TAB}");
                //AutoItX.Send("moyai25");
                //AutoItX.Send("{ENTER}");

                //XAuto.Click<RadioButtonElement>(windowPass, "Require password-based");
                //XAuto.ClickByXpath<RadioButtonElement>(windowPass, "\\RadioButton[@Name='Require password-based authentication (make sure this box is always checked!)']");
                //XAuto.ClickByXpath<ButtonElement>(windowPass, "\\Button[@Name='OK']");



                //IntPtr main = XAutoNative.FindWindow(null, "TightVNC Server: Set Passwords");
                //IntPtr main2 = XAutoNative.FindWindow(null, "TightVNC Setup");

                //// 2️⃣ Tìm ô Edit con
                //// Bạn có thể lặp nhiều lần nếu có nhiều ô Edit:
                //IntPtr edit1 = XAutoNative.FindWindowEx(main, IntPtr.Zero, "Edit", null);
                //IntPtr edit2 = XAutoNative.FindWindowEx(main, edit1, "Edit", null);

                //// 3️⃣ Gửi text trực tiếp
                //if (edit1 != IntPtr.Zero)
                //{
                //    XAutoNative.ForceForeground(main2);
                //    Thread.Sleep(199);
                //    XAutoNative.SetForegroundWindow(edit1);

                //    XAutoNative.SendUnicodeText("moyai25");
                //    //var sim = new InputSimulator();
                //    //sim.Keyboard.TextEntry("moyai25");
                //    //Thread.Sleep(1000);
                //    //AutoItX.Send("moyai25");
                //    //XAutoNative.SendMessage(edit1, XAutoNative.WM_SETTEXT, IntPtr.Zero, "moyai25");
                //    //XAutoNative.SendMessage(edit1, XAutoNative.EM_SETSEL, (IntPtr)(-1), "moyai25");
                //    //XAutoNative.SendMessage(edit1, XAutoNative.EM_REPLACESEL, (IntPtr)1, "moyai25");
                //    //XAutoNative.SendTextStrong("moyai25");


                //    //Console.WriteLine("Đã set text vào ô Edit thành công.");
                //}
                //if (edit2 != IntPtr.Zero)
                //{
                //    XAutoNative.ForceForeground(edit2);
                //    //XAutoNative.SendText("moyai25");
                //    //AutoItX.Send("moyai25");
                //    //XAutoNative.SendMessage(edit2, XAutoNative.WM_SETTEXT, IntPtr.Zero, "moyai25");
                //    //Console.WriteLine("Đã set text vào ô Edit thành công.");
                //}










                // ProcessHelper.Kill("TabTip");
                // Process p = System.Diagnostics.Process.Start(@"C:\Program Files\Common Files\Microsoft Shared\ink\TabTip.exe");
                //var keyboardWindow=  UIElementFinder.GetWindowDeep("Microsoft Text Input Application", 6);
                // XAuto.Click<ButtonElement>(keyboardWindow, "m");
                // XAuto.Click<ButtonElement>(keyboardWindow, "o");
                // XAuto.Click<ButtonElement>(keyboardWindow, "y");


                // XAuto.SetText<TextBoxElement>("Set Passwords", "\\Pane[@AutomationId='1084']", "moyai25");
                //XAuto.SetText<TextBoxElement>("Set Passwords", "\\Pane[@AutomationId='1087']", "moyai25");

                //XAuto.SetTextBox<TextBoxElement>(window, "Password for VNC viewer");
                //DebugHelper.DumpAll();
                //UIElementFinder.DeepFindElement("Set Passwords", "Required password-base authentication");
                // AutomationElement messageBoxWindow = XAuto.WaitAllWindowUntilExists<LabelElement>("TightVNC Setup", "Files in Use");
                //XAuto.ClickUntilExistsByClass<TreeViewElement>(window, "SysTreeView32");
                return true;
                XAuto.Click<ButtonElement>("Setup TightVNC 2.6", "Close");
                window = XAuto.GetWindow("TightVNC GUI");
                XAuto.Click<ButtonElement>(window, "OK");

            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Install TightVNC failed: {ex.StackTrace}");
                return false;
            }
            Logger.WriteLine("[END] installed completed TightVNC");
            return true;
        }

        public static bool Uninstall()
        {
            Logger.WriteLine("[START] TightVNC is already installed. Uninstalling...");
            try
            {
                //"TightVNC-setup.exe /uninstall /silent"
                var window = XAuto.GetWindow("TightVNC Setup");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Remove");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Remove");
                // AutomationElement messageBoxWindow= XAuto.WaitAllWindowUntilExists<LabelElement>("TightVNC Setup", "Files in Use");
                XAuto.Click<ButtonElement>("TightVNC Setup", "Finish");

                //AutomationElement messageBoxWindow= XAuto.WaitAllWindowUntilExists<LabelElement>("TightVNC Setup", "The setup must update files or services");
                // XAuto.Click<ButtonElement>(messageBoxWindow, "OK");


            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Uninstall TightVNC failed: {ex.StackTrace}");
                return false;
            }
            Logger.WriteLine("[END] TightVNC is uninstall completed!");
            return true;
        }
    }
}
