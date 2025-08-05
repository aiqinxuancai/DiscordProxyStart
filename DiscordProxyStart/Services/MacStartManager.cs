using DiscordProxyStart.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace DiscordProxyStart.Servers
{
    internal class MacStartManager
    {

        public static void Start()
        {
            var proxy = GetProxy();

            Console.WriteLine($"Start Discord Proxy:{proxy}");
            //ProcessStartInfo startInfo = new ProcessStartInfo();
            //startInfo.FileName = $"/Applications/Discord.app/Contents/MacOS/Discord";
            //startInfo.Arguments = $"--proxy-server={proxy}";
            //startInfo.UseShellExecute = true;
            //startInfo.EnvironmentVariables.Add("http_proxy", proxy); 
            //startInfo.EnvironmentVariables.Add("https_proxy", proxy);

            //Process.Start(startInfo);

           
            //string commands = $"env http_proxy=\"{proxy}\" https_proxy=\"{proxy}\" /Applications/Discord.app/Contents/MacOS/Discord --proxy-server=\"{proxy}\"\n";

            string commands = $"export http_proxy=\"{proxy}\"\nexport https_proxy=\"{proxy}\"\n/Applications/Discord.app/Contents/MacOS/Discord --proxy-server=\"{proxy}\"\n";

            Process process = new Process();

            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"-c \"{commands}\"";

            process.Start();

            process.WaitForExit();

        }

        private static string GetProxy()
        {
            //读取本地ini配置，读取
            var iniPath = Path.Combine(AppContext.BaseDirectory, "Config.ini");
            IniFile ini = new IniFile(iniPath);
            if (!File.Exists(iniPath))
            {
                var firstIni = """
                    [Config]
                    Proxy=
                    """;
                File.WriteAllText(iniPath, firstIni);

                //创建文件
                throw new Exception(LocalizationManager.Instance.GetString("ConfigNotFound"));
            }

            var proxy = ini.GetValue("Config", "Proxy");

            if (string.IsNullOrEmpty(proxy))
            {
                throw new Exception(LocalizationManager.Instance.GetString("ProxyNotSet"));
            }

            return proxy.Replace("\"", "").Trim();
        }


    }
}
