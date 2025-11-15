using ApSetting.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApSetting
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NotificationManager.Start();

            var options = new EdgeOptions();
            options.AddArgument("start-maximized");

            IWebDriver driver = new EdgeDriver(options);

            NotificationManager.ShowMessage("Open setting site");
            
            driver.Navigate().GoToUrl("http://web.setting/");

            Thread.Sleep(6000);

            driver.DoAction(1,"Click to Button Login",
                By.Id("div_MainLogin"),
                e => e.Click());

            driver.DoAction(2,"Input password",
                By.Id("IndexLoginPwd_Show"),
                e => e.SendKeys("Nhungth8x@!"));

            driver.DoAction(3,"Summit Login",
             By.Id("btLoginForm"),
             e => e.Click());

            driver.DoAction(4,"Click about menu",
                By.Id("mainNewMenuMobileAbout"),
                e => e.Click());

            driver.SaveAllScreenshots(Path.Combine(AppContext.BaseDirectory, "CameraAutomationLog.png"));
            driver.Quit();
            NotificationManager.Close();
        }
    }
}
