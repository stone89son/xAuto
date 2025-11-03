using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xAuto.Core.Helpers
{
    public class RegistryHelper
    {  
        
        
        /// <summary>
       /// Xóa một registry key bằng đường dẫn đầy đủ kiểu "HKEY_LOCAL_MACHINE\SOFTWARE\TightVNC".
       /// Trả về true nếu đã xóa ở ít nhất một view (64/32), false nếu không tìm thấy.
       /// Ném UnauthorizedAccessException nếu không có quyền ghi.
       /// </summary>
        public static bool DeleteRegistryKey(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("fullPath không được rỗng.", nameof(fullPath));

            // Tách hive và subpath
            int firstSlash = fullPath.IndexOf('\\');
            string hiveName = firstSlash >= 0 ? fullPath.Substring(0, firstSlash) : fullPath;
            string subPath = firstSlash >= 0 ? fullPath.Substring(firstSlash + 1) : string.Empty;

            if (string.IsNullOrEmpty(subPath))
                throw new ArgumentException("fullPath phải chứa cả hive và subkey, ví dụ: HKEY_LOCAL_MACHINE\\SOFTWARE\\TightVNC");

            // Map hive name sang RegistryHive
            RegistryHive hive;
            switch (hiveName.ToUpperInvariant())
            {
                case "HKEY_LOCAL_MACHINE":
                case "HKLM":
                    hive = RegistryHive.LocalMachine;
                    break;
                case "HKEY_CURRENT_USER":
                case "HKCU":
                    hive = RegistryHive.CurrentUser;
                    break;
                case "HKEY_CLASSES_ROOT":
                case "HKCR":
                    hive = RegistryHive.ClassesRoot;
                    break;
                case "HKEY_USERS":
                case "HKU":
                    hive = RegistryHive.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                case "HKCC":
                    hive = RegistryHive.CurrentConfig;
                    break;
                default:
                    throw new ArgumentException("Root hive không hợp lệ: " + hiveName);
            }

            bool deletedAny = false;

            // Thử xóa trong cả 64-bit và 32-bit views (để xử lý Wow6432Node)
            foreach (RegistryView view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
            {
                try
                {
                    using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
                    {
                        // Tách parent path và target key name
                        int lastSlash = subPath.LastIndexOf('\\');
                        string parentPath = lastSlash >= 0 ? subPath.Substring(0, lastSlash) : string.Empty;
                        string targetName = lastSlash >= 0 ? subPath.Substring(lastSlash + 1) : subPath;

                        using (RegistryKey parent = string.IsNullOrEmpty(parentPath)
                            ? baseKey
                            : baseKey.OpenSubKey(parentPath, writable: true))
                        {
                            if (parent == null)
                            {
                                // không tồn tại ở view này
                                //Console.WriteLine($"[Info] Không tìm thấy parent '{parentPath}' trong view {view}.");
                                continue;
                            }

                            using (RegistryKey target = parent.OpenSubKey(targetName))
                            {
                                if (target == null)
                                {
                                    // không tồn tại ở view này
                                    //Console.WriteLine($"[Info] Không tìm thấy key '{targetName}' trong view {view}.");
                                    continue;
                                }
                            }

                            // Nếu tới đây, target tồn tại -> xóa cả cây
                            parent.DeleteSubKeyTree(targetName);
                            Logger.WriteLine($"Deleted: {hiveName}\\{subPath} (view: {view})");
                            deletedAny = true;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Không đủ quyền ở view này -> ném ra (caller sẽ biết)
                    throw;
                }
                catch (ArgumentException ex)
                {
                   
                }
                catch (Exception ex)
                {
                    Logger.WriteLine($"Error delete regedit (view {view}): {ex.Message}");
                }
            }

            if (!deletedAny)
            {
                //Console.WriteLine($"Key '{fullPath}' không tìm thấy trong cả 64-bit và 32-bit registry views.");
            }

            return deletedAny;
        }

        /// <summary>
        /// Kiểm tra xem tiến trình hiện tại có chạy với quyền admin hay không.
        /// (Tiện ích, dùng nếu bạn muốn cảnh báo user.)
        /// </summary>
        public static bool IsRunningAsAdministrator()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
