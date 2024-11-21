using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace DiscordProxyStart.Utils
{

    public class IniFile
    {
        private Dictionary<string, Section> _sections;
        private string _filePath;

        public IniFile(string filePath)
        {
            _filePath = filePath;
            _sections = new Dictionary<string, Section>();
            Load();
        }

        public class Section
        {
            public Dictionary<string, string> Values { get; set; }
            public Dictionary<string, string> Comments { get; set; }
            public string SectionComment { get; set; }

            public Section()
            {
                Values = new Dictionary<string, string>();
                Comments = new Dictionary<string, string>();
            }
        }

        public void Load()
        {
            _sections.Clear();
            if (!File.Exists(_filePath)) return;

            Section currentSection = null;
            string currentComment = "";

            foreach (string line in File.ReadAllLines(_filePath, Encoding.UTF8))
            {
                string trimmedLine = line.Trim();

                // 跳过空行
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                // 处理注释行
                if (trimmedLine.StartsWith("#") || trimmedLine.StartsWith(";"))
                {
                    currentComment = trimmedLine;
                    continue;
                }

                // 处理节
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    string sectionName = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentSection = new Section();
                    if (!string.IsNullOrEmpty(currentComment))
                    {
                        currentSection.SectionComment = currentComment;
                        currentComment = "";
                    }
                    _sections[sectionName] = currentSection;
                    continue;
                }

                // 处理键值对
                if (currentSection != null)
                {
                    int equalPos = trimmedLine.IndexOf('=');
                    if (equalPos > 0)
                    {
                        string key = trimmedLine.Substring(0, equalPos).Trim();
                        string value = trimmedLine.Substring(equalPos + 1).Trim();
                        currentSection.Values[key] = value;
                        if (!string.IsNullOrEmpty(currentComment))
                        {
                            currentSection.Comments[key] = currentComment;
                            currentComment = "";
                        }
                    }
                }
            }
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(_filePath, false, Encoding.UTF8))
            {
                foreach (var section in _sections)
                {
                    // 写入节注释
                    if (!string.IsNullOrEmpty(section.Value.SectionComment))
                    {
                        writer.WriteLine(section.Value.SectionComment);
                    }

                    // 写入节名
                    writer.WriteLine($"[{section.Key}]");

                    // 写入键值对和注释
                    foreach (var pair in section.Value.Values)
                    {
                        if (section.Value.Comments.ContainsKey(pair.Key))
                        {
                            writer.WriteLine(section.Value.Comments[pair.Key]);
                        }
                        writer.WriteLine($"{pair.Key}={pair.Value}");
                    }

                    writer.WriteLine();
                }
            }
        }

        public string GetValue(string section, string key, string defaultValue = "")
        {
            if (_sections.ContainsKey(section) && _sections[section].Values.ContainsKey(key))
            {
                return _sections[section].Values[key];
            }
            return defaultValue;
        }

        public void SetValue(string section, string key, string value)
        {
            if (!_sections.ContainsKey(section))
            {
                _sections[section] = new Section();
            }
            _sections[section].Values[key] = value;
        }

        public void SetComment(string section, string key, string comment)
        {
            if (!_sections.ContainsKey(section))
            {
                _sections[section] = new Section();
            }
            _sections[section].Comments[key] = comment;
        }

        public void SetSectionComment(string section, string comment)
        {
            if (!_sections.ContainsKey(section))
            {
                _sections[section] = new Section();
            }
            _sections[section].SectionComment = comment;
        }
    }

}
