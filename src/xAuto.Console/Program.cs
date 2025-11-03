using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xAuto.Core;
using xAuto.Core.Controls;
using xAuto.Core.Enums;
using xAuto.Core.Helpers;
using static xAuto.Core.Sys;
namespace xAuto
{
    public class Program
    {
        static void EndAutomation(bool isPass, string msg)
        {
            if (!isPass)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Logger.WriteLine(msg);
                Console.ResetColor();
                Console.WriteLine("\n");
            }
            else
            {
                Console.ResetColor();
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
                Logger.WriteLine("➞Installed successfully.");
                Console.ResetColor();
                Console.WriteLine("\n");
                //Logger.WriteLine("Automation ended. Press Enter to exit.");
            }
           
        }
        static void Main(string[] args)
        {
            //KeyboardBlocker.BlockPhysicalKeyboard();
            bool isInstallOpenVPN = OpenVPNInstall.Run();
            if (!isInstallOpenVPN)
            {
                EndAutomation(false, "OpenVPN installation failed.");
                //Logger.WriteLine("Retrying OpenVPN installation...");
                return;
            }
            bool isInstallTightVNC = TightVNCInstall.Run();
            if (!isInstallTightVNC)
            {
                EndAutomation(false, "TightVNC installation failed.");
                return;
            }

            EndAutomation(true, string.Empty);
            //KeyboardBlocker.UnblockPhysicalKeyboard();
            Console.ReadLine();
            //string tightvncPath = @"D:\Projects\xAuto\setup\tightvnc-2.8.85-gpl-setup-64bit.msi";
            //string novaLCTPath = @"D:\Projects\xAuto\setup\NovaLCT V5.7.1.exe";
        }
    }
}
