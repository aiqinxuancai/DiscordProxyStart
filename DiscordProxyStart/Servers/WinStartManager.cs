using DiscordProxyStart.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordProxyStart.Servers
{
    internal class WinStartManager
    {

        public static void Start()
        {

            
            var iniPath = Path.Combine(AppContext.BaseDirectory, "Config.ini");

            //如果配置文件中有配置
            var setupPath = SetupPathHelper.GetInstallPath("Discord"); 
            if (File.Exists(iniPath))
            {
                var path = IniFileHelper.GetIniValue(iniPath, "Config", "Path");
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    setupPath = path;
                }
            }

            if (string.IsNullOrEmpty(setupPath) || !Directory.Exists(setupPath))
            {
                var exePath = SetupPathHelper.GetDiscordExePath("Discord");
                
                if (File.Exists(exePath))
                {
                    PortableStart(exePath);
                }
                else
                {
                    MsgBox.Show("Error", "没有找到Discord安装目录", MsgBoxButtons.OK);
                    Debug.WriteLine("没有找到安装目录");
                    return;
                }
                
            }
            else
            {
                //正常安装由此启动
                NormalStart(setupPath);
            }

        }

        
        /// <summary>
        /// 绿色版启动
        /// </summary>
        /// <param name="exePath"></param>
        private static void PortableStart(string exePath)
        {
            try
            {
                var setupPath = Path.GetDirectoryName(Path.GetDirectoryName(exePath));
                Debug.WriteLine("正在准备启动...");
                var appPath = GetAppPaths(setupPath);
                var proxy = GetProxy();

#if DEBUG
                proxy = "http://127.0.0.1:1081";
#endif

                Debug.WriteLine(proxy);
  
                var appPathFirst = appPath.lastVersion;

                if (!string.IsNullOrEmpty(proxy) && Directory.Exists(appPathFirst))
                {
                    Debug.WriteLine("启动进程...");
                    var process = new Process();
                    process.StartInfo.FileName = exePath;
                    process.StartInfo.Arguments = $@"--proxy-server={proxy} --user-data-dir={Path.Combine(setupPath, "data") }";
                    process.StartInfo.WorkingDirectory = appPathFirst;
                    //process.StartInfo.EnvironmentVariables["HTTPS_PROXY"] = proxy;
                    //process.StartInfo.EnvironmentVariables["HTTP_PROXY"] = proxy;

                    process.Start();

                    //TODO 等待升级窗口出现并销毁
                    WaitUpdater(setupPath, appPath);


                    //TODO 检查是否有更新
                }
                else
                {
                    MsgBox.Show("Error", $"{exePath}\n{proxy}\n{appPathFirst}\n{setupPath}", MsgBoxButtons.OK);
                }

            }
            catch (Exception ex)
            {
                MsgBox.Show("Error", ex.Message, MsgBoxButtons.OK);
            }
        }

        private static void NormalStart(string setupPath)
        {
            //var updatePath = Path.Combine(setupPath, "Update.exe");

            //if (!File.Exists(updatePath))
            //{
            //    MsgBox.Show("Error", "没有找到入口程序Update.exe", MsgBoxButtons.OK);
            //    Debug.WriteLine("没有找到入口程序Update.exe");
            //    return;
            //}

            try
            {
               
                var appPaths = GetAppPaths(setupPath);
                var proxy = GetProxy();

#if DEBUG
                proxy = "http://127.0.0.1:1081";
#endif
                Debug.WriteLine($"正在准备启动 {appPaths.lastVersion} {proxy} ...");
                var appPath = Path.Combine(setupPath, appPaths.lastVersion);


                if (!string.IsNullOrEmpty(proxy) && Directory.Exists(appPath))
                {
                    Debug.WriteLine("启动进程...");
                    var process = new Process();
                    process.StartInfo.FileName = Path.Combine(appPath, "Discord.exe"); ;
                    //process.StartInfo.Arguments = $"--processStart Discord.exe --a=--proxy-server={proxy}";
                    process.StartInfo.Arguments = $"--proxy-server={proxy}";
                    process.StartInfo.WorkingDirectory = appPath;
                    process.StartInfo.EnvironmentVariables["HTTPS_PROXY"] = proxy;
                    process.StartInfo.EnvironmentVariables["HTTP_PROXY"] = proxy;

                    process.StartInfo.EnvironmentVariables["https_proxy"] = proxy;
                    process.StartInfo.EnvironmentVariables["http_proxy"] = proxy;

                    process.Start();

                    //TODO 等待升级窗口出现并销毁
                    WaitUpdater(setupPath, appPaths);
                }
                else
                {
                    Debug.WriteLine("无法启动...");
                }

            }
            catch (Exception ex)
            {
                MsgBox.Show("Error", ex.Message, MsgBoxButtons.OK);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupPath"></param>
        /// <returns></returns>
        private static void WaitUpdater(string setupPath, (string lastVersion, List<string> allVersion) appPaths)
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


            bool hasUpdate = false;
            while (User32.FindWindow("Chrome_WidgetWin_1", "Discord Updater") > 0) //等待更新窗口销毁
            {
                //检查是否有更新！如果有更新，在进程结束后主动结束一次新创建的Discord
                if (!hasUpdate && HasUpdate(setupPath, appPaths))
                {
                    hasUpdate = true;
                    Debug.WriteLine("正在进行更新");
                }
                Task.Delay(100).Wait();
            }
            Debug.WriteLine("Discord Updater已销毁");
        }

        /// <summary>
        /// 是否有升级？
        /// </summary>
        /// <param name="setupPath"></param>
        /// <returns></returns>
        private static bool HasUpdate(string setupPath, (string lastVersion, List<string> allVersion) appPaths)
        {
            var currentPath = GetAppPaths(setupPath);
            if (currentPath.lastVersion != appPaths.lastVersion)
            {
                return true;
            }
            return false;
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

        private static (string lastVersion, List<string> allVersion) GetAppPaths(string setupPath)
        {
            var subDirs = Directory.GetDirectories(setupPath);
            List<string> paths = new List<string>();
            Version lastVersion = new Version(0, 0, 0, 0);
            string lastVersionAppPath = string.Empty;
            foreach (var subDir in subDirs)
            {
                var pathName = Path.GetFileName(subDir);
                var pathVersion = pathName.Replace("app-", "");
                if (Version.TryParse(pathVersion, out var version))
                {
                    if (version > lastVersion)
                    {
                        lastVersion = version;
                        lastVersionAppPath = pathName;
                    }
                    paths.Add(subDir);
                }
                Console.WriteLine(pathName);
            }
            return (lastVersionAppPath, paths);
        }

        ///// <summary>
        ///// 获取Discord目录的下所有app-开头目录并检查是否有version.dll,如果没有则复制
        ///// </summary>
        ///// <param name="setupPath"></param>
        ///// <returns></returns>
        //private static bool CopyVersionDll(string setupPath)
        //{
        //    var appPath = GetAppPath(setupPath);
        //    //自动下载缺少的dll？
        //    var dllFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "version.dll");
        //    foreach (var path in appPath)
        //    {
        //        var newPath = Path.Combine(path, "version.dll");
        //        if (!File.Exists(newPath))
        //        {
        //            File.Copy(dllFilePath, newPath, true); //文件有可能被占用
        //        }
        //    }
        //    return appPath.Count > 0;
        //}
    }
}
