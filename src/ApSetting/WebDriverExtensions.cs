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
using SeleniumExtras.WaitHelpers;
using System.Runtime.CompilerServices;

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
            bool isWaitVisible = true,
            int timeoutInSeconds = 60 * 10)
        {
            NotificationManager.ShowMessage($"#{screenshotIndex} {actionDescription}");
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} #{screenshotIndex} {actionDescription}");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            var element = wait.Until(drv =>
            {
                try
                {
                    var el = drv.FindElement(by);
                    if (isWaitVisible)
                    {
                        var elClickable = (el.Displayed && el.Enabled) ? el : null;
                        if (elClickable != null)
                        {
                            return elClickable;
                        }
                    }
                    else if (el != null)
                    {
                        return el;
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
            //var element = wait.Until(ExpectedConditions.ElementToBeClickable(by));


            //((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);

            Capture(driver, screenshotIndex, actionDescription, "BEFORE");
            action(element);
            Capture(driver, screenshotIndex, actionDescription, "AFTER");
        }

        public static void DoConfirm(this IWebDriver driver,
            int screenshotIndex,
            string description,
            By by,
            string expectedValue,
            bool isWaitVisible = true,
            int timeoutInSeconds = 60)
        {
            NotificationManager.ShowMessage($"#{screenshotIndex} {description}");
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} #{screenshotIndex} {description}");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));

            // Đợi element xuất hiện
            var element = wait.Until(drv =>
            {
                try
                {
                    var el = drv.FindElement(by);

                    if (isWaitVisible)
                    {
                        return (el.Displayed && el.Enabled) ? el : null;
                    }
                    else
                    {
                        return el;
                    }
                }
                catch
                {
                    return null;
                }
            });

            // Screenshot BEFORE
            Capture(driver, screenshotIndex, description, "CONFIRM-BEFORE");

            // Lấy giá trị thực tế
            string actualValue = GetElementValue(element);

            // Screenshot AFTER
            Capture(driver, screenshotIndex, description, "CONFIRM-AFTER");

            // So sánh
            bool isMatch = string.Equals(actualValue?.Trim(), expectedValue?.Trim(), StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"[CONFIRM] Expected = '{expectedValue}', Actual = '{actualValue}', Result = {isMatch}");

            if (!isMatch)
            {
                throw new Exception(
                    $"[DoConfirm FAILED] #{screenshotIndex} {description}\n" +
                    $"Expected: '{expectedValue}'\n" +
                    $"Actual:   '{actualValue}'");
            }
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

        public static string GetElementValue(IWebElement element)
        {
            if (element == null)
                return null;

            string tag = element.TagName.ToLower();

            if (tag == "input" || tag == "textarea")
            {
                string v2 = element.GetAttribute("value");
                if (!string.IsNullOrEmpty(v2)) return v2;
            }

            string text = element.Text;
            if (!string.IsNullOrEmpty(text)) return text;

            string inner = element.GetAttribute("innerText");
            if (!string.IsNullOrEmpty(inner)) return inner;

            string tc = element.GetAttribute("textContent");
            if (!string.IsNullOrEmpty(tc)) return tc;

            return "";
        }


        private static bool IsVisible(IWebDriver driver, IWebElement el)
        {
            return (bool)((IJavaScriptExecutor)driver).ExecuteScript(
                "var elem = arguments[0];" +
                "var box = elem.getBoundingClientRect();" +
                "return (" +
                "box.width > 0 && box.height > 0 && " +
                "elem === document.elementFromPoint(box.left + box.width/2, box.top + box.height/2));",
                el);
        }
    }
}
