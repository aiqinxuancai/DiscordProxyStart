using DiscordProxyStart.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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



            string commands = $"export http_proxy={proxy}\nexport http_proxy={proxy}\n/Applications/Discord.app/Contents/MacOS/Discord --proxy-server={proxy}\n";

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

            if (!File.Exists(iniPath))
            {
                var firstIni = """
                    [Config]
                    Proxy=
                    """;
                File.WriteAllText(iniPath, firstIni);

                //创建文件
                throw new Exception("没有找到配置文件Config.ini，已自动生成，请在Proxy=后填写代理地址");
            }

            var proxy = IniFileHelper.GetIniValue(iniPath, "Config", "Proxy");

            if (string.IsNullOrEmpty(proxy))
            {
                throw new Exception("Config.ini中未设置代理地址");
            }

            return proxy.Replace("\"", "").Trim();
        }


    }
}
