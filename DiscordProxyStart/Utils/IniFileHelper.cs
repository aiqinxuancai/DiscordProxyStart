using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordProxyStart.Utils
{
    /// <summary>
    /// ini文件操作，不想使用API，所以直接文本方式封装
    /// </summary>
    internal class IniFileHelper
    {
        public static string GetIniValue(string filePath, string sectionName, string keyName)
        {

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 判断是否为指定的 Section
                        if (line.Trim().StartsWith($"[{sectionName}]"))
                        {
                            // 读取 Key 对应的值
                            while ((line = reader.ReadLine()) != null && line.Trim().StartsWith(keyName))
                            {
                                string[] parts = line.Split('=', 2);
                                if (parts.Length == 2)
                                {
                                    // 返回值
                                    string value = parts[1].Trim();
                                    return value;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {


            }


            // 如果未找到则返回空字符串
            return string.Empty;
        }

        public static void SetIniValue(string filePath, string sectionName, string keyName, string value)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            bool sectionExists = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Trim().StartsWith($"[{sectionName}]"))
                {
                    sectionExists = true;

                    bool keyExists = false;
                    for (int j = i + 1; j < lines.Length; j++)
                    {
                        if (lines[j].Trim().StartsWith(keyName))
                        {
                            lines[j] = $"{keyName}={value}";
                            keyExists = true;
                            break;
                        }
                        else if (lines[j].Trim().StartsWith("["))
                        {
                            break;
                        }
                    }

                    if (!keyExists)
                    {
                        List<string> newLines = new List<string>();
                        newLines.AddRange(lines.Take(i + 1));
                        newLines.Add($"{keyName}={value}");
                        newLines.AddRange(lines.Skip(i + 1));
                        lines = newLines.ToArray();
                    }

                    break;
                }
            }

            if (!sectionExists)
            {
                List<string> newLines = new List<string>();
                newLines.AddRange(lines);
                newLines.Add($"[{sectionName}]");
                newLines.Add($"{keyName}={value}");
                lines = newLines.ToArray();
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}
