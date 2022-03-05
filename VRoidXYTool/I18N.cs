using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using System.Linq;

namespace VRoidXYTool
{
    /// <summary>
    /// 插件的多语言配置
    /// </summary>
    public static class I18N
    {
        /// <summary>
        /// 当前插件所使用的语言
        /// </summary>
        public static SystemLanguage NowLanguage;

        /// <summary>
        /// 当前语言使用的字典
        /// </summary>
        private static Dictionary<string, string> NowLanguageDict;

        /// <summary>
        /// 后备语言使用的字典
        /// </summary>
        private static Dictionary<string, string> FallbackLanguageDict;

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            TryLoadLanguage(SystemLanguage.English, out FallbackLanguageDict);
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public static void SetLanguage(SystemLanguage language)
        {
            Dictionary<string, string> dict;
            if (TryLoadLanguage(language, out dict))
            {
                XYTool.Inst.PluginLanguage.Value = language;
                NowLanguageDict = dict;
                NowLanguage = language;
            }
            else
            {
                // 如果无法读取到目标语言，则使用内置的语言
                if (language == SystemLanguage.Chinese || language == SystemLanguage.ChineseTraditional)
                {
                    SetLanguage(SystemLanguage.ChineseSimplified);
                }
                else
                {
                    SetLanguage(SystemLanguage.English);
                }
            }
        }

        /// <summary>
        /// 尝试加载语言文件
        /// </summary>
        private static bool TryLoadLanguage(SystemLanguage language, out Dictionary<string, string> dict)
        {
            dict = new Dictionary<string, string>();
            // 检查外部路径是否有语言文件，如果有，则读取外部路径的文件，如果没有，则读取内置的
            FileInfo langFile = new FileInfo($"{Paths.ConfigPath}/XYToolI18N/{language}.txt");
            if (!langFile.Directory.Exists)
            {
                langFile.Directory.Create();
            }
            if (langFile.Exists)
            {
                try
                {
                    var lines = File.ReadAllLines(langFile.FullName);
                    dict = CreateDictByLines(lines.ToList());
                    return true;
                }
                catch
                {
                    // 如果出现异常，则跳过并读取内置的
                }
            }
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"VRoidXYTool.PluginI18N.{language}.txt");
            if (stream != null)
            {
                try
                {
                    List<string> list = new List<string>();
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        while (true)
                        {
                            string line = sr.ReadLine();
                            // 如果读取不到内容了，则退出循环
                            if (line == null)
                            {
                                break;
                            }
                            list.Add(line);
                        }
                    }
                    dict = CreateDictByLines(list);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 创建语言字典
        /// </summary>
        private static Dictionary<string, string> CreateDictByLines(List<string> lines)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string line in lines)
            {
                string[] kv = line.Split(new char[] { '=' }, 2);
                // 如果当前行的分割结果长度不为2，则忽略
                if (kv.Length != 2)
                {
                    continue;
                }
                // 转换换行符
                string value = kv[1].Replace("\\n", "\n");
                // 添加文本到字典
                dict[kv[0]] = value;
            }
            return dict;
        }

        /// <summary>
        /// 根据key获取翻译文本
        /// </summary>
        public static string Get(string key)
        {
            if (NowLanguageDict.ContainsKey(key))
            {
                return NowLanguageDict[key];
            }
            else if (FallbackLanguageDict.ContainsKey(key))
            {
                return FallbackLanguageDict[key];
            }
            else
            {
                Debug.LogWarning($"VRoidXYTool的I18N缺失，language:{NowLanguage}, key:{key}");
                FallbackLanguageDict[key] = key;
                return key;
            }
        }

        /// <summary>
        /// 翻译的扩展方法
        /// </summary>
        public static string Translate(this string key)
        {
            return Get(key);
        }
    }
}
