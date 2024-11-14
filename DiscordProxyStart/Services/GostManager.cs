using DiscordProxyStart.Utils;
using System;
using System.Diagnostics;
using System.IO;
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

        public void StartProxy(string socksProxyAddress, int localHttpPort)
        {
            KillExistingGostProcess();
            var startInfo = new ProcessStartInfo
            {
                FileName = gostPath,
                Arguments = $"-L=http://127.0.0.1:{localHttpPort} -F={socksProxyAddress}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Debug.WriteLine(startInfo.Arguments);
            var currentProcess = new Process { StartInfo = startInfo };
            currentProcess.Start();
            //currentProcess.WaitForExit();
            Thread.Sleep(3000);
        }
    }


}
