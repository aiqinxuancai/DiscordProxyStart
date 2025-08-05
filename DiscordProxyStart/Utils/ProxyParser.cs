using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordProxyStart.Utils
{

    /// <summary>
    /// 代理解析类
    /// </summary>
    public class ProxyParser
    {
        /// <summary>
        /// 代理协议类型 "http", "https", "socks", "socks4", "socks5" 等
        /// </summary>
        public string Type { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public bool RequiresAuthentication => !string.IsNullOrEmpty(Username) || !string.IsNullOrEmpty(Password);

        public string Username { get; set; }

        public string Password { get; set; }

        public ProxyParser()
        {
            // 默认类型
            Type = "http";
        }

        /// <summary>
        /// 从代理 URL 解析代理信息
        /// </summary>
        /// <param name="proxyUrl">代理 URL，例如 socks5://username:password@ip:port 或 https://ip:port</param>
        /// <returns>Proxy 实例</returns>
        public static ProxyParser Parse(string proxyUrl)
        {
            if (string.IsNullOrWhiteSpace(proxyUrl))
                throw new ArgumentException(LocalizationManager.Instance.GetString("ProxyUrlEmpty"), nameof(proxyUrl));

            Uri uri;
            try
            {
                // 如果没有协议前缀，Uri 解析会失败，所以需要添加一个虚拟协议
                if (!proxyUrl.Contains("://"))
                {
                    proxyUrl = "http://" + proxyUrl;
                }
                uri = new Uri(proxyUrl);
            }
            catch (UriFormatException ex)
            {
                throw new ArgumentException(LocalizationManager.Instance.GetString("ProxyUrlInvalid"), ex);
            }

            var proxy = new ProxyParser();

            // 设置代理类型
            proxy.Type = uri.Scheme.ToLower();

            // 设置主机和端口
            proxy.Host = uri.Host;
            proxy.Port = uri.Port;

            // 设置用户名和密码
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                var userInfo = uri.UserInfo.Split(':');
                if (userInfo.Length > 0)
                {
                    proxy.Username = Uri.UnescapeDataString(userInfo[0]);
                }
                if (userInfo.Length > 1)
                {
                    proxy.Password = Uri.UnescapeDataString(userInfo[1]);
                }
            }

            return proxy;
        }

        /// <summary>
        /// 将代理信息转换为标准代理 URL 字符串
        /// </summary>
        /// <returns>代理 URL 字符串</returns>
        public override string ToString()
        {
            string scheme = Type.ToLower();

            string userInfo = string.Empty;
            if (RequiresAuthentication)
            {
                string username = Uri.EscapeDataString(Username ?? string.Empty);
                string password = Uri.EscapeDataString(Password ?? string.Empty);
                userInfo = $"{username}:{password}@";
            }

            return $"{scheme}://{userInfo}{Host}:{Port}";
        }

    }
}
