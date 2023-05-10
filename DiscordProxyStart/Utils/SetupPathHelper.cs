using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordProxyStart.Utils
{
    internal class SetupPathHelper
    {
        public static string GetInstallPath(string appName)
        {
            string installPath = string.Empty;

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{appName}");
                installPath = registryKey.GetValue("InstallLocation").ToString();
            }
            catch
            {
                // 处理异常情况，例如 Discord 未安装在默认位置时
            }

            if (string.IsNullOrWhiteSpace(installPath))
            {
                // 处理无法找到安装目录的情况
            }
            else
            {
                // 返回 Discord 的安装目录
                return installPath;
            }


            return string.Empty; // 找不到安装目录时返回 null
        }
    }
}
