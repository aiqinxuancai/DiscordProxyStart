# DiscordProxyStart
Experimental project to quickly start a Discord process with a proxy.

This project uses the proxy method provided by https://github.com/aiqinxuancai/discord-proxy, simplifying operations without the need to copy the version.dll or modify shortcuts, reducing potential issues.

### Getting Started
After downloading the release, extract everything to any directory, modify the Config.ini file in the directory to add your local proxy address, and then run the DiscordProxyStart.exe file to start Discord with proxy support.

Example Config.ini configuration:
```ini
[Config]
Proxy=http://127.0.0.1:1080
```

You can press Win+R, type shell:startup, and press enter. Then, hold the Alt key and drag the DiscordProxyStart.exe to this directory to enable startup launch.

---

In version 0.0.14, support for SOCKS proxy format has been added: `socks5://ip:port`, `socks://username:password@ip:port`, etc.