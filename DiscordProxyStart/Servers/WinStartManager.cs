using DiscordProxyStart.Utils;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                var customPath = IniFileHelper.GetIniValue(iniPath, "Config", "Path");
                if (!string.IsNullOrWhiteSpace(customPath))
                {
                    setupPath = customPath;
                }
            }

            if (string.IsNullOrEmpty(setupPath))
            {
                MsgBox.Show("Error", "没有找到Discord安装目录#1", MsgBoxButtons.OK);
                Debug.WriteLine("没有找到安装目录");
                return;
            }

            if (!Directory.Exists(setupPath))
            {
                //安装目录不存在，使用另外的方式检查Discord.exe
                var exePath = SetupPathHelper.GetDiscordExePath("Discord");
                
                if (File.Exists(exePath))
                {
                    PortableStart(exePath);
                }
                else
                {
                    MsgBox.Show("Error", "没有找到Discord安装目录#2", MsgBoxButtons.OK);
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
                var appPath = GetAppPath(setupPath);
                var proxy = GetProxy();
                var copyResult = CopyVersionDll(setupPath);

                //TODO 自动gost转换socks代理？

                var appPathFirst = appPath.lastVersionPath;

                if (!string.IsNullOrEmpty(proxy) && copyResult && Directory.Exists(appPathFirst))
                {
                    Debug.WriteLine($"启动进程...{exePath}");

                    var cmd = $@"--proxy-server={proxy} --user-data-dir={Path.Combine(setupPath, "data")}";
                    Debug.WriteLine($"启动进程参数...{cmd}");
                    var process = new Process();
                    process.StartInfo.FileName = exePath;
                    process.StartInfo.Arguments = cmd;
                    process.StartInfo.WorkingDirectory = appPathFirst;
                    process.Start();

                    //TODO 等待升级窗口出现并销毁
                    WaitUpdater(setupPath);
                }
                else
                {
                    MsgBox.Show("Error", $"{exePath}\n{proxy}\n{copyResult}\n{appPathFirst}\n{setupPath}", MsgBoxButtons.OK);
                }

            }
            catch (Exception ex)
            {
                MsgBox.Show("Error#1", ex.Message, MsgBoxButtons.OK);
            }
        }

        private static void NormalStart(string setupPath)
        {
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
                var appPaths = GetAppPath(setupPath);
                var proxy = GetProxy();
                var copyResult = CopyVersionDll(setupPath);

                //TODO 自动gost转换socks代理？

                var appPath = appPaths.lastVersionPath; //TODO 需要取最新版本 并且循环
                if (!string.IsNullOrEmpty(proxy) && copyResult && Directory.Exists(appPath))
                {
                    Debug.WriteLine($"启动进程 {updatePath} ...");

     
                    
                    var process = new Process();
                    process.StartInfo.FileName = updatePath;
                    process.StartInfo.Arguments = $"--processStart {GetPathDiscordName(appPath)} --a=--proxy-server={proxy}";
                    process.StartInfo.WorkingDirectory = appPath;
                    process.Start();

                    //TODO 等待升级窗口出现并销毁
                    WaitUpdater(setupPath);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
                MsgBox.Show("Error#2", ex.Message, MsgBoxButtons.OK);
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

        private static (List<string> allPaths, string lastVersionPath) GetAppPath(string setupPath)
        {
            var subDirs = Directory.GetDirectories(setupPath);
            List<string> paths = new List<string>();
            Version lastVersion = new Version(0,0,0,0);
            string lastPath = "";


            foreach (var subDir in subDirs)
            {
                var dirName = Path.GetFileName(subDir);
                Console.WriteLine(dirName);
                if (dirName.StartsWith("app-")) //理论上有一个，遇到升级可能有两个，特殊情况可能存在空文件夹
                {
                    var verStr = dirName.Replace("app-", "");
                    if (Version.TryParse(verStr, out var version))
                    {
                        if (version > lastVersion)
                        {
                            lastVersion = version;
                            lastPath = subDir;
                        }


                        paths.Add(subDir);
                    }




                    
                }
            }

            return (paths, lastPath);
        }


        private static string GetPathDiscordName(string path)
        {
            var mainExeFileName = string.Empty;
            if (File.Exists(Path.Combine(path, "Discord.exe")))
            {
                mainExeFileName = "Discord.exe";
            }
            else if (File.Exists(Path.Combine(path, "DiscordCanary.exe")))
            {
                mainExeFileName = "DiscordCanary.exe";
            }
            return mainExeFileName;
        }

        /// <summary>
        /// 获取Discord目录的下所有app-开头目录并检查是否有version.dll,如果没有则复制
        /// </summary>
        /// <param name="setupPath"></param>
        /// <returns></returns>
        private static bool CopyVersionDll(string setupPath)
        {
            var appPath = GetAppPath(setupPath);

            //TODO 自动下载缺少的dll？
            var runCount = 0;
            foreach (var path in appPath.allPaths)
            {

                var discordExeName = GetPathDiscordName(path);
                if (!string.IsNullOrEmpty(discordExeName))
                {
                    runCount++;
                    var discordExePath = Path.Combine(path, discordExeName);
                    var exeMachineType = PEUtils.GetExecutableMachineType(discordExePath);
                    var targetDllPath = Path.Combine(path, "version.dll");
                    var dllFilePath = Path.Combine(AppContext.BaseDirectory, "x86", "version.dll");

                    if (exeMachineType == PEUtils.MachineType.IMAGE_FILE_MACHINE_AMD64)
                    {
                        dllFilePath = Path.Combine(AppContext.BaseDirectory, "x64", "version.dll");
                    }


                    if (!File.Exists(dllFilePath))
                    {
                        throw new FileNotFoundException($"没有找到本地的 {dllFilePath}");
                    }
                    if (!File.Exists(discordExePath))
                    {
                        throw new FileNotFoundException($"目标路径没有 {discordExePath} ？？？");
                    }

                    if (!File.Exists(targetDllPath))
                    {
                        File.Copy(dllFilePath, targetDllPath, true); //TODO 文件有可能被占用?
                    }
                    else
                    {
                        //用于处理已经存在旧版本的情况
                        var nowDllInfo = new FileInfo(targetDllPath);
                        var dllInfo = new FileInfo(dllFilePath);
                        if (dllInfo.Length != nowDllInfo.Length)
                        {
                            File.Copy(dllFilePath, targetDllPath, true); //TODO 文件有可能被占用?
                        }
                    }
                }

                
            }


            return runCount > 0;

        }
    }
}
