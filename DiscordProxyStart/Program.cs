using DiscordProxyStart.Utils;
using System.Diagnostics;
using System.Net;

namespace DiscordProxyStart
{
    internal class Program
    {
        static void Main(string[] args)
        {

#if DEBUG

#endif

            var setupPath = SetupPathHelper.GetInstallPath("Discord");

            if (string.IsNullOrEmpty(setupPath) || !Directory.Exists(setupPath))
            {
                MsgBox.Show("Error", "没有找到Discord安装目录", MsgBoxButtons.OK);
                Debug.WriteLine("没有找到安装目录");
                return;
            }

            var updatePath = Path.Combine(setupPath, "Update.exe");

            if (!File.Exists(updatePath))
            {
                MsgBox.Show("Error", "没有找到入口程序Update.exe", MsgBoxButtons.OK);
                Debug.WriteLine("没有找到入口程序Update.exe");
                return;
            }

            try
            {
                var appPath = GetAppPath(setupPath);
                var proxy = GetProxy();
                var copyResult = CopyVersionDll(setupPath);

                //TODO 自动gost转换socks代理？

                if (!string.IsNullOrEmpty(proxy) && copyResult && Directory.Exists(appPath))
                {
                    var process = new Process();
                    process.StartInfo.FileName = updatePath;
                    process.StartInfo.Arguments = $"--processStart Discord.exe --a=--proxy-server={proxy}";
                    process.StartInfo.WorkingDirectory = appPath;


                    process.Start();
                }
                
            }
            catch (Exception ex)
            {
                MsgBox.Show("Error", ex.Message, MsgBoxButtons.OK);
            }
        }


        private static string GetProxy()
        {
            //读取本地ini配置，读取
            var iniPath = Path.Combine(AppContext.BaseDirectory, "Config.ini");

            if (!File.Exists(iniPath))
            {
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

        private static string GetAppPath(string setupPath)
        {
            var subDirs = Directory.GetDirectories(setupPath);

            var dllFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "version.dll");

            if (File.Exists(dllFilePath))
            {
                foreach (var subDir in subDirs)
                {
                    var dirName = Path.GetFileName(subDir);
                    Console.WriteLine(dirName);
                    if (dirName.StartsWith("app-")) //理论上有一个
                    {
                        return subDir;
                    }
                }
            }
            return string.Empty;
        }

        private static bool CopyVersionDll(string setupPath)
        {
            var appPath = GetAppPath(setupPath);

            //自动下载缺少的dll？

            var dllFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "version.dll");
            var newPath = Path.Combine(appPath, "version.dll");

            File.Copy(dllFilePath, newPath, true); //文件有可能被占用
            return true;

        }
    }
}