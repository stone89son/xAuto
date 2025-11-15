using ApSetting.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ApSetting
{
    public static class WebDriverExtensions
    {
        private static List<Bitmap> _screenshots = new List<Bitmap>();

        // Waits until the element is visible, enabled, and scrolls it into view
        public static IWebElement XFindElement(this IWebDriver driver, By by, int timeoutInSeconds = 60)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            var element = wait.Until(drv =>
            {
                try
                {
                    var el = drv.FindElement(by);
                    return (el.Displayed && el.Enabled) ? el : null;
                }
                catch
                {
                    return null;
                }
            });

            // Scroll element into view
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

            return element;
        }

        public static void DoAction(this IWebDriver driver,
            int screenshotIndex,
            string actionDescription,
            By by,
            Action<IWebElement> action,
            int timeoutInSeconds = 40)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            var element = wait.Until(drv =>
            {
                try
                {
                    var el = drv.FindElement(by);
                    var elClickable = (el.Displayed && el.Enabled) ? el : null;
                    if (elClickable != null)
                    {
                        return elClickable;
                    }
                    // Element not yet present in DOM, return null to continue waiting
                    return null;
                }
                catch
                {
                    // Return null to continue waiting
                    return null;
                }
            });

            //((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            NotificationManager.ShowMessage($"#{screenshotIndex} {actionDescription}");
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} #{screenshotIndex} {actionDescription}");
            Capture(driver, screenshotIndex, actionDescription, "BEFORE");
            action(element);
            Capture(driver, screenshotIndex, actionDescription, "AFTER");
        }

        private static void Capture(IWebDriver driver, int screenshotIndex, string actionDescription, string when)
        {
            Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
            using (var ms = new System.IO.MemoryStream(ss.AsByteArray))
            {
                Bitmap original = new Bitmap(ms);

                // Tạo ảnh mới có thêm thanh bên trên (height = 70px)
                int bannerHeight = 70;
                Bitmap bmpWithBanner = new Bitmap(original.Width, original.Height + bannerHeight);
                using (Graphics g = Graphics.FromImage(bmpWithBanner))
                {
                    // Fill thanh màu xám
                    g.Clear(Color.White); // nền tổng thể
                    g.FillRectangle(Brushes.DarkViolet, 0, 0, bmpWithBanner.Width, bannerHeight);

                    // Vẽ số thứ tự lớn ở góc trái
                    string numberText = $"#{screenshotIndex}";
                    g.DrawString(numberText, new Font("Arial", 32, FontStyle.Bold), Brushes.White, new PointF(10, 10));

                    // Ghi text trên thanh
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string logText = $"[{when}] {actionDescription}";
                    g.DrawString($"{logText} | {time}", new Font("Arial", 16, FontStyle.Bold), Brushes.White, new PointF(120, 25));
                    // Vẽ ảnh gốc bên dưới thanh
                    g.DrawImage(original, 0, bannerHeight);
                   
                }

                _screenshots.Add(new Bitmap(bmpWithBanner));
                original.Dispose();
            }
        }
        public static void SaveAllScreenshots(this IWebDriver driver, string filePath)
        {
            if (_screenshots.Count == 0) return;

            int width = 0, height = 0;
            foreach (var img in _screenshots)
            {
                if (img.Width > width) width = img.Width;
                height += img.Height;
            }

            Bitmap final = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(final))
            {
                g.Clear(Color.White);
                int offset = 0;
                foreach (var img in _screenshots)
                {
                    g.DrawImage(img, 0, offset);
                    offset += img.Height;
                }
            }

            final.Save(filePath, ImageFormat.Png);

            foreach (var img in _screenshots) img.Dispose();
            final.Dispose();
            _screenshots.Clear();
        }
    }
}
