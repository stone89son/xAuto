using ManagedNativeWifi;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace ApSetting
{
    public class WifiHelper
    {
        /* 
         Plan (pseudocode, detailed):
         1. Add `IsConnected(string ssid)`:
            - For each interface returned by `NativeWifi.EnumerateInterfaces()` where `InterfaceInfo.State == InterfaceState.Connected`:
              - Call `NativeWifi.GetCurrentConnection(interfaceId)` which returns `(ActionResult, connectionInfo)`.
              - If result is `ActionResult.Success` and `connectionInfo.Ssid` matches `ssid` (use case-insensitive comparison), return true.
            - If no match found return false.
         2. Add `WaitForConnected(string ssid, int timeoutMilliseconds = 15000, int pollIntervalMilliseconds = 500)`:
            - Start a `Stopwatch`.
            - Loop until elapsed time exceeds `timeoutMilliseconds`:
              - Call `IsConnected(ssid)`. If true return true.
              - Sleep `pollIntervalMilliseconds`.
            - After timeout return false.
         3. Update `Connect(string ssid, string password)` to:
            - Create profile using existing `CreateProfile`.
            - Call `NativeWifi.ConnectNetwork(...)` to start connection.
            - If `ConnectNetwork` returns false, return false immediately.
            - Otherwise call `WaitForConnected(ssid, ...)` and return its result.
         4. Keep error messages concise and in Vietnamese, and avoid calling missing APIs.
        */

        //public static string ExportProfileXml()
        //{
        //    var adapter = GetAdapter();
        //    var profile = NativeWifi.EnumerateProfiles()
        //        .FirstOrDefault(p => p.Name.Equals(ssid, StringComparison.OrdinalIgnoreCase));

        //}

        private static string RunCmd(string cmd)
        {
            var p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/C " + cmd;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.Start();
            p.WaitForExit();

            return p.StandardOutput.ReadToEnd();
        }

        private static string ToHexadecimalString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var bytes = System.Text.Encoding.ASCII.GetBytes(input);
            return BitConverter.ToString(bytes).Replace("-", "");
        }
        //netsh wlan export profile name="F82F65502F66" folder="C:\temp"
        private static string BuildProfileXmlString(string ssid, string password, bool isAuto)
        {
            // Trường hợp không có password
            if (string.IsNullOrEmpty(password))
            {
                return $@"
                    <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                      <name>{ssid}</name>
                      <SSIDConfig>
                        <SSID><name>{ssid}</name></SSID>
                      </SSIDConfig>
                      <connectionType>ESS</connectionType>
                      <connectionMode>{(isAuto ? "auto" : "manual")}</connectionMode>
                      <MSM>
                        <security>
                          <authEncryption>
                            <authentication>open</authentication>
                            <encryption>none</encryption>
                            <useOneX>false</useOneX>
                          </authEncryption>
                        </security>
                      </MSM>
                    </WLANProfile>";
            }
            //WPA2PSK
            // Trường hợp có password WPA2
            return $@"
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>{ssid}</name>
  <SSIDConfig>
    <SSID>
      <hex>{ToHexadecimalString(ssid)}</hex>
      <name>{ssid}</name>
    </SSID>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>auto</connectionMode>
  <MSM>
    <security>
      <authEncryption>
        <authentication>WPA3SAE</authentication>
        <encryption>AES</encryption>
        <useOneX>false</useOneX>
      </authEncryption>
      <sharedKey>
        <keyType>passPhrase</keyType>
        <protected>false</protected>
        <keyMaterial>{password}</keyMaterial>
      </sharedKey>
    </security>
  </MSM>
</WLANProfile>";
        }

        private static InterfaceInfo GetAdapter()
        {
            var adapter = NativeWifi.EnumerateInterfaces().FirstOrDefault();
            if (adapter == null) throw new Exception("Không tìm thấy card WiFi!");
            return adapter;
        }
        /// <summary>
        /// Tạo mới profile wifi, xóa profile cũ nếu trùng
        /// </summary>
        public static bool CreateProfile(string ssid, string password, bool isAuto = false)
        {
            var adapter = GetAdapter();
            try
            {
                string profileXml = BuildProfileXmlString(ssid, password, isAuto);
                var temp = Path.Combine(Path.GetTempPath(), "wifi.xml");
                File.WriteAllText(temp, profileXml);
                string result = RunCmd($@"netsh wlan add profile filename=""{temp}"" user=all");
                File.Delete(temp);
                if (result.Contains("追加された"))
                {
                    return true;
                }

                //bool result = NativeWifi.SetProfile(
                //    adapter.Id,
                //    ProfileType.AllUser,
                //    profileXml,
                //    ssid,
                //    true
                //);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo profile WiFi: {ex.Message}");
                return false;
            }

        }

        public static bool Ping(string ip, int timeoutMs = 3000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(ip, timeoutMs);

                    if (reply.Status == IPStatus.Success)
                        return true;

                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public static async Task<bool> PingAsync(string ip, int timeoutMs = 3000)
        {

            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(ip, timeoutMs);

                if (reply.Status == IPStatus.Success)
                    return true;

                // trả false để RetryHelper retry tiếp
                return false;
            }
        }


        /// <summary>
        /// Kết nối WiFi với retry và trả lỗi rõ ràng
        /// </summary>
        public static bool Connect(string ssid, string password)
        {
            try
            {
                var adapter = GetAdapter();

                bool isDeleted = DeleteProfile(ssid);
                if (!isDeleted)
                {
                    return false;
                    throw new Exception($"Cannot delete profile {ssid}!");
                }

                bool isCreated = CreateProfile(ssid, password);
                if (!isCreated)
                {
                    return false;
                    throw new Exception($"Cannot create profile {ssid}!");
                }

                bool isConnectedRequest = NativeWifi.ConnectNetwork(
                           adapter.Id,
                           ssid,
                           BssType.Infrastructure,
                           null
                       );
                // Đợi trạng thái kết nối thực tế
                return isConnectedRequest;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem đang kết nối tới SSID cụ thể hay không
        /// </summary>
        private static bool IsConnected(string ssid)
        {
            var connectedSsids = NativeWifi.EnumerateConnectedNetworkSsids()?.FirstOrDefault(e => e.ToString() == ssid);
            if (connectedSsids != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Chờ tới khi kết nối tới `ssid` hoặc timeout (ms) hết
        /// </summary>
        public static bool WaitForConnected(string ssid, int timeoutSeconds = 120,
            int pollIntervalSeconds = 1000)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutSeconds)
            {
                if (IsConnected(ssid))
                    return true;
                Thread.Sleep(pollIntervalSeconds);
            }
            return false;
        }

        /// <summary>
        /// Xóa tất cả profile trên tất cả adapter
        /// </summary>
        public static void DeleteAllProfiles()
        {
            var profiles = NativeWifi.EnumerateProfiles();

            foreach (var p in profiles)
            {
                Console.WriteLine($"Xóa profile '{p.Name}'");
                NativeWifi.DeleteProfile(p.InterfaceInfo.Id, p.Name);
            }
        }

        /// <summary>
        /// Xóa tất cả profile trên tất cả adapter
        /// </summary>
        public static bool DeleteProfile(string profileName)
        {
            var targetProfile = NativeWifi.EnumerateProfiles()
                .Where(x => profileName.Equals(x.Name, StringComparison.Ordinal))
                .FirstOrDefault();

            if (targetProfile is null)
                return true;

            return NativeWifi.DeleteProfile(
                interfaceId: targetProfile.InterfaceInfo.Id,
                profileName: profileName);
        }

        public static void ShowConnectedNetworkInformation()
        {
            foreach (var interfaceId in NativeWifi.EnumerateInterfaces()
                .Where(x => x.State is InterfaceState.Connected)
                .Select(x => x.Id))
            {
                // Following methods work only with connected wireless interfaces.
                var (result, cc) = NativeWifi.GetCurrentConnection(interfaceId);
                if (result is ActionResult.Success)
                {
                    Trace.WriteLine($"Profile: {cc.ProfileName}");
                    Trace.WriteLine($"SSID: {cc.Ssid}");
                    Trace.WriteLine($"PHY type: 802.11{cc.PhyType.ToProtocolName()}");
                    Trace.WriteLine($"Authentication algorithm: {cc.AuthenticationAlgorithm}");
                    Trace.WriteLine($"Cipher algorithm: {cc.CipherAlgorithm}");
                    Trace.WriteLine($"Signal quality: {cc.SignalQuality}");
                    Trace.WriteLine($"Rx rate: {cc.RxRate} Kbps");
                    Trace.WriteLine($"Tx rate: {cc.TxRate} Kbps");
                }

                // GetRealtimeConnectionQuality method works only on Windows 11 24H2.
                var (result1, rcq) = NativeWifi.GetRealtimeConnectionQuality(interfaceId);
                if (result1 is ActionResult.Success)
                {
                    Trace.WriteLine($"PHY type: 802.11{rcq.PhyType.ToProtocolName()}");
                    Trace.WriteLine($"Link quality: {rcq.LinkQuality}");
                    Trace.WriteLine($"Rx rate: {rcq.RxRate} Kbps");
                    Trace.WriteLine($"Tx rate: {rcq.TxRate} Kbps");
                    Trace.WriteLine($"MLO connection: {rcq.IsMultiLinkOperation}");

                    if (rcq.Links.Count > 0)
                    {
                        var link = rcq.Links[0];
                        Trace.WriteLine($"RSSI: {link.Rssi}");
                        Trace.WriteLine($"Frequency: {link.Frequency} MHz");
                        Trace.WriteLine($"Bandwidth: {link.Bandwidth} MHz");
                    }
                }
                else if (result is ActionResult.NotSupported)
                {
                    var (result2, rssi) = NativeWifi.GetRssi(interfaceId);
                    if (result2 is ActionResult.Success)
                    {
                        Trace.WriteLine($"RSSI: {rssi}");
                    }
                }
            }
        }
    }
}
