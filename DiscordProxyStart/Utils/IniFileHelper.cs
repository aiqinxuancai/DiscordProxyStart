using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                string fileContent = File.ReadAllText(filePath, Encoding.UTF8);

                fileContent = Regex.Replace(fileContent, @"\r\n?|\n", "\n");

                using (StringReader reader = new StringReader(fileContent))
                {
                    string line;
                    bool isInSection = false;

                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (!isInSection)
                        {
                            if (line.StartsWith($"[{sectionName}]"))
                            {
                                isInSection = true;
                            }
                        }
                        else
                        {
                            if (line.StartsWith("["))
                            {
                                break;
                            }
                            if (line.StartsWith(keyName))
                            {
                                string[] parts = line.Split(new[] { '=' }, 2);
                                return parts.Length == 2 ? parts[1].Trim() : string.Empty;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

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
                        List<string> newLines = [.. lines.Take(i + 1), $"{keyName}={value}", .. lines.Skip(i + 1)];
                        lines = newLines.ToArray();
                    }

                    break;
                }
            }

            if (!sectionExists)
            {
                List<string> newLines = [.. lines, $"[{sectionName}]", $"{keyName}={value}"];
                lines = newLines.ToArray();
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
        }
    }
}
