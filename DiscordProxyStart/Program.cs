using DiscordProxyStart.Servers;
using DiscordProxyStart.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace DiscordProxyStart
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                WinStartManager.Start();
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Failed the test and cannot be used
                // MacStartManager.Start();
            }
            
        }

        
    }
}