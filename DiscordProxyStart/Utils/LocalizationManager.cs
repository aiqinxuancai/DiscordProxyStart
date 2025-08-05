using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace DiscordProxyStart.Utils
{
    public class LocalizationManager
    {
        private static readonly Lazy<LocalizationManager> _instance = new Lazy<LocalizationManager>(() => new LocalizationManager());
        private readonly ResourceManager _resourceManager;

        private LocalizationManager()
        {
            _resourceManager = new ResourceManager("DiscordProxyStart.Resources.Strings", typeof(Program).Assembly);
        }

        public static LocalizationManager Instance => _instance.Value;

        public string GetString(string name)
        {
            return _resourceManager.GetString(name, Thread.CurrentThread.CurrentUICulture) ?? name;
        }

        public string GetString(string name, params object[] args)
        {
            var format = _resourceManager.GetString(name, Thread.CurrentThread.CurrentUICulture) ?? name;
            return string.Format(format, args);
        }
    }
}
