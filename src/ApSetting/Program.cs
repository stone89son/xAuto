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
        static async Task Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                //NotificationManager.Start();

                WifiHelper.DeleteAllProfiles();
                List<(bool, bool)> isSuccessTests = new List<(bool, bool)>();
                int maxAttempts1 = 0, maxAttempts2 = 0;
                for (int i = 0; i < 100; i++)
                {
                    bool isConnected = await RetryHelperAsync.DoAsync((attempt) =>
                    {
                        if (attempt != 1)
                        {
                            Console.WriteLine($"Connect retry {attempt-1}");
                            if (attempt > maxAttempts1)
                            {
                                maxAttempts1 = attempt;
                            }
                        }
                        bool isConnectedWifi = WifiHelper.Connect("F82F65502F66-2G", "tm22t60jh01126");
                        return Task.FromResult(isConnectedWifi);
                    }, 10);


                    bool isPingedOk = await RetryHelperAsync.DoAsync(attempt =>
                    {
                        if (attempt != 1)
                        {
                            Console.WriteLine($"Ping retry {attempt-1}");
                            if (attempt > maxAttempts2)
                            {
                                maxAttempts2 = attempt;
                            }
                        }
                        bool isPingOk = WifiHelper.Ping("192.168.128.1");
                        return Task.FromResult(isPingOk);

                    }, 200);

                    isSuccessTests.Add((isConnected, isPingedOk));
                    Console.WriteLine($"{(i + 1)} Connected to WiFi: {isConnected} , Ping: {isPingedOk}");
                }
                Console.WriteLine($@"Total Connected Count: {isSuccessTests.Count}, 
True:{isSuccessTests.Count(e => e.Item1 == true)},
False:{isSuccessTests.Count(e => e.Item1 == false)}
Max Attemps:{maxAttempts1}");

                Console.WriteLine($@"Total Ping Count: {isSuccessTests.Count}, 
True:{isSuccessTests.Count(e => e.Item2 == true)},
False:{isSuccessTests.Count(e => e.Item2 == false)}
Max Attemps:{maxAttempts2}");
                //NotificationManager.Close();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
