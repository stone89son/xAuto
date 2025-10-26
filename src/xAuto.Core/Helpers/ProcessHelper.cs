using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.IO;

namespace xAuto.Core.Helpers
{
    public class ProcessHelper
    {
        public static void KillForMsi(string namePart)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("msiexec");
                foreach (var p in processes)
                {
                    p.Kill();
                }
            }
            catch
            {
            }
        }
        public static void Kill(string namePart)
        {
            var processes = System.Diagnostics.Process.GetProcesses()
                .Where(p => p.ProcessName.Contains(namePart));
            foreach (var process in processes)
            {
                //Logger.WriteLine(process.ProcessName);
                process.Kill();
                process.WaitForExit();
            }
        }
        public static bool RunPassFill()
        {
            string psexecPath = Path.Combine(AppContext.BaseDirectory, "ThirdParties", "PsExec64.exe"); // hoặc chỉ "PsExec64.exe" nếu ở PATH hoặc cùng thư mục
            string targetExe = Path.Combine(AppContext.BaseDirectory, "ThirdParties", "type.exe");

            // Lưu ý: 1 cặp " quanh targetExe -> \"{targetExe}\"
            string args = $"-accepteula -i -s \"{targetExe}\"";

            var psi = new ProcessStartInfo
            {
                FileName = psexecPath,
                Arguments = args,
                UseShellExecute = false,           // để có thể redirect
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas",               // chạy với quyền admin
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                Console.WriteLine("ExitCode: " + p.ExitCode);
                Console.WriteLine("Stdout:\n" + stdout);
                Console.WriteLine("Stderr:\n" + stderr);
            }
            return true;
        }

        public static (bool result, string output, string error) RunCommand(string command)
        {
            string output;
            string error;
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = new Process())
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                }
                return (string.IsNullOrEmpty(error), output, null);
            }
            catch (Exception ex)
            {
                output = string.Empty;
                error = ex.Message;
                return (false, output, error);
            }
        }
    }
}
