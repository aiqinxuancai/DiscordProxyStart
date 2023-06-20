using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            }

            if (!string.IsNullOrWhiteSpace(installPath))
            {
                return installPath;
            }

            return string.Empty;
        }

        public static string GetDiscordExePath(string appName)
        {

            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Classes\{appName}\shell\open\command");
                var discordExePath = registryKey.GetValue(null).ToString();

                if (discordExePath != null)
                {
                    var regex = new Regex(@"\""(.*?)\""", RegexOptions.Multiline);
                    var match = regex.Match(discordExePath);
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }

                }
            }
            catch
            {
               
            }


            return string.Empty;
        }
    }
}
