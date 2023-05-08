using DiscordProxyStart.Utils;
using System.Diagnostics;
using System.IO;
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
                Debug.WriteLine("正在准备启动...");
                var appPath = GetAppPath(setupPath);
                var proxy = GetProxy();
                var copyResult = CopyVersionDll(setupPath);

                //TODO 自动gost转换socks代理？

                if (!string.IsNullOrEmpty(proxy) && copyResult && Directory.Exists(appPath.FirstOrDefault()))
                {
                    Debug.WriteLine("启动进程...");
                    var process = new Process();
                    process.StartInfo.FileName = updatePath;
                    process.StartInfo.Arguments = $"--processStart Discord.exe --a=--proxy-server={proxy}";
                    process.StartInfo.WorkingDirectory = appPath.FirstOrDefault();
                    process.Start();

                    //TODO 等待升级窗口出现并销毁
                    WaitUpdater(setupPath);
                }
                
            }
            catch (Exception ex)
            {
                MsgBox.Show("Error", ex.Message, MsgBoxButtons.OK);
            }
        }

        private static void WaitUpdater(string setupPath)
        {


            int fullWaitCount = 0;
            //等待窗口出现
            while (User32.FindWindow("Chrome_WidgetWin_1", "Discord Updater") == 0)
            {
                Task.Delay(100).Wait();
                fullWaitCount += 100;
                if (fullWaitCount > 30 * 1000) //30秒没发现进程有启动
                {
                    //没有正确启动 返回
                    return;
                }
            }

            Debug.WriteLine("Discord Updater已创建");

            while (User32.FindWindow("Chrome_WidgetWin_1", "Discord Updater") > 0) //等待更新窗口销毁
            {
                CopyVersionDll(setupPath);
                Task.Delay(100).Wait();
            }

            Debug.WriteLine("Discord Updater已销毁");
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

        private static List<string> GetAppPath(string setupPath)
        {
            var subDirs = Directory.GetDirectories(setupPath);
            List<string> paths = new List<string>();
           var dllFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "version.dll");

            if (File.Exists(dllFilePath))
            {
                foreach (var subDir in subDirs)
                {
                    var dirName = Path.GetFileName(subDir);
                    Console.WriteLine(dirName);
                    if (dirName.StartsWith("app-")) //理论上有一个，遇到升级可能有两个
                    {
                        paths.Add(subDir);
                    }
                }
            }
            return paths;
        }

        /// <summary>
        /// 获取Discord目录的下所有app-开头目录并检查是否有version.dll,如果没有则复制
        /// </summary>
        /// <param name="setupPath"></param>
        /// <returns></returns>
        private static bool CopyVersionDll(string setupPath)
        {
            var appPath = GetAppPath(setupPath);

            //自动下载缺少的dll？

            var dllFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "version.dll");


            foreach (var path in appPath)
            {
                var newPath = Path.Combine(path, "version.dll");
                if (!File.Exists(newPath))
                {
                    File.Copy(dllFilePath, newPath, true); //文件有可能被占用
                }
            }


            return true;

        }
    }
}