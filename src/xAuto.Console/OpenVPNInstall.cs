using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using xAuto.Core;
using xAuto.Core.Controls;
using xAuto.Core.Helpers;
using static xAuto.Core.Sys;
namespace xAuto
{
    public class OpenVPNInstall
    {
        private static string _openVPNPath = @"D:\Projects\xAuto\setup\OpenVPN-2.6.15-I001-amd64.msi";

        public static bool Run()
        {
            Logger.WriteLine("[START] Close OpenVPN window that was previously open");
            ProcessHelper.KillForMsi("OpenVPN");
            ProcessHelper.Kill("openvpn");
            Logger.WriteLine("[END] Close OpenVPN window that was previously open");
            return Install();
        }

        public static bool Install()
        {
            try
            {
                Logger.WriteLine("[OPEN] OpenVPN setup");
                Process.Start(_openVPNPath);
                Sleep(1);

                var window = XAuto.WaitForWindow("Setup OpenVPN 2.6");
                if (window == null)
                {
                    Logger.WriteLine("Cannot find OpenVPN setup window");
                    return false;
                }

                LabelElement maintenanceLabel = XAuto.FindControl<LabelElement>(window, "OpenVPN Maintenance");
                if (maintenanceLabel != null)
                {

                    bool isUnstall = Uninstall();
                    if (!isUnstall)
                    {
                        return false;
                    }

                    Logger.WriteLine("[OPEN] OpenVPN setup");
                    Process.Start(_openVPNPath);
                    Sleep(2);
                    window = XAuto.WaitForWindow("Setup OpenVPN 2.6");
                }
                else
                {
                    Logger.WriteLine("OpenVPN Maintenance Label found.");
                }

                Logger.WriteLine("[START] install OpenVPN");

                XAuto.Click<ButtonElement>(window, "Install Now");
                window = XAuto.WaitUntilExists<ButtonElement>("Setup OpenVPN 2.6", "Completed");
                XAuto.Click<ButtonElement>("Setup OpenVPN 2.6", "Close");
                try
                {
                    //Sometime no show dialog
                    window = XAuto.GetWindow("OpenVPN GUI");
                    XAuto.Click<ButtonElement>(window, "OK");
                }
                catch { 
                
                }
            }

            catch (Exception ex)
            {
                Logger.WriteLine($"Install OpenVPN failed: {ex.StackTrace}");
                return false;
            }
            Logger.WriteLine("[END] installed completed OpenVPN");
            return true;
        }

        public static bool Uninstall()
        {
            Logger.WriteLine("[START] OpenVPN is already installed. Uninstalling...");
            try
            {
                var window = XAuto.GetWindow("Setup OpenVPN 2.6");
                XAuto.Click<RadioButtonElement>(window, "Remove");
                XAuto.Click<ButtonElement>(window, "Continue");
                XAuto.WaitUntilExists<LabelElement>("Setup OpenVPN 2.6", "OpenVPN Removing Completed");
                XAuto.Click<ButtonElement>("Setup OpenVPN 2.6", "Close");
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"Uninstall OpenVPN failed: {ex.StackTrace}");
                return false;
            }
            Logger.WriteLine("[END] OpenVPN is uninstall completed!");
            return true;
        }
    }
}
