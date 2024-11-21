using DiscordProxyStart.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace DiscordProxyStart.Services
{

    public class GostManager
    {

        private static GostManager _instance;
        private static readonly object _lock = new object();

        private readonly string gostPath;

        public static GostManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new GostManager($@"{AppContext.BaseDirectory}gost.exe");
                        }
                    }
                }
                return _instance;
            }
        }

        public GostManager(string gostPath)
        {
            this.gostPath = gostPath;
        }

        private void KillExistingGostProcess()
        {
            var processes = Process.GetProcessesByName("gost");
            foreach (var process in processes)
            {
                try
                {
                    string processPath = Path.GetFullPath(process.MainModule?.FileName ?? string.Empty);

                    if (string.Equals(processPath, gostPath, StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit();
                    }
                }
                catch (Exception ex) 
                {
                    SimpleLogger.Instance.Info(@$"[KillExistingGostProcessError]{ex}");
                }
            }
        }

        public int StartProxy(string socksProxyAddress)
        {
            KillExistingGostProcess();

            // 获取一个可用的随机端口
            int port = GetAvailablePort();

            var startInfo = new ProcessStartInfo
            {
                FileName = gostPath,
                Arguments = $"-L=http://127.0.0.1:{port} -F={socksProxyAddress}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Debug.WriteLine(startInfo.Arguments);
            var currentProcess = new Process { StartInfo = startInfo };
            currentProcess.Start();
            Thread.Sleep(1500);

            return port;
        }

        private int GetAvailablePort()
        {
            Random random = new Random();
            while (true)
            {
                // 在63000-64000之间随机选择一个端口
                int port = random.Next(63000, 64000);

                // 检查端口是否被占用
                if (!IsPortInUse(port))
                {
                    return port;
                }
            }
        }

        private bool IsPortInUse(int port)
        {
            try
            {
                // 检查TCP端口
                using (var tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port))
                {
                    tcpListener.Start();
                    tcpListener.Stop();
                    return false;
                }
            }
            catch (SocketException)
            {
                return true;
            }
        }

    }


}
