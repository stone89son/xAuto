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

namespace xAuto.Core.Helpers
{
    public static class WaitHelper
    {
        /// <summary>
        /// Repeatedly execute the provided function until it returns true or timeout expires.
        /// </summary>
        /// <param name="action">The action to execute repeatedly.</param>
        /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
        /// <param name="pollIntervalMs">Delay between attempts.</param>
        /// <param name="timeoutMessage">Optional message to print when timeout occurs.</param>
        /// <returns>True if action succeeded before timeout, false otherwise.</returns>
        public static bool WaitAppInstallUntilTimeout(
            Func<bool> action)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < Config.AppInstallTimeout)
            {
                if (action())
                {
                    var remainingTime = Config.AppInstallTimeout - stopwatch.Elapsed.TotalSeconds;
                    if (remainingTime <= 0)
                    {
                       Logger.WriteLine("WaitAppInstallUntilTimeout timeout expired.");
                        return false;
                    }
                    return true;
                }
                Sys.Sleep(Config.PollInterval);
            }
            return false;
        }

        public static bool WaitWindowUntilTimeout(
         Func<bool> action)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < Config.AppOpenTimeout)
            {
                if (action())
                {
                    var remainingTime = Config.AppOpenTimeout - stopwatch.Elapsed.TotalSeconds;
                    if (remainingTime <= 0)
                    {
                        return false;
                    }
                    return true;
                }
                Sys.Sleep(Config.PollInterval);
            }
            return false;
        }

        /// <summary>
        /// Repeatedly execute the provided function until it returns true or timeout expires.
        /// </summary>
        /// <param name="action">The action to execute repeatedly.</param>
        /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
        /// <param name="pollIntervalMs">Delay between attempts.</param>
        /// <param name="timeoutMessage">Optional message to print when timeout occurs.</param>
        /// <returns>True if action succeeded before timeout, false otherwise.</returns>
        
        public static bool WaitElementUntilTimeout(
            Func<bool> action)
        {
            var stopwatch = Stopwatch.StartNew();
            while (stopwatch.Elapsed.TotalSeconds < Config.FindControlTimeout)
            {
                if (action())
                {
                    var remainingTime = Config.FindControlTimeout - stopwatch.Elapsed.TotalSeconds;
                    if (remainingTime <= 0)
                    {
                        return false;
                    }
                    return true;
                }
                Sys.Sleep(Config.PollInterval);
            }
            return false;
        }

    }
}
