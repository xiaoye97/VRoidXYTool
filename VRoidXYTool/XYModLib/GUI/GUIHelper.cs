using System;
using UnityEngine;
using System.Collections.Generic;

namespace XYModLib
{
    public static class GUIHelper
    {
        private static Dictionary<string, string> strCache = new Dictionary<string, string>();

        /// <summary>
        /// 清空缓存
        /// </summary>
        public static void ClearCache()
        {
            strCache.Clear();
        }

        /// <summary>
        /// 整形数据的输入GUI
        /// </summary>
        /// <returns></returns>
        public static int IntTextGUI(int num, string key, int width = 0, int min = int.MinValue, int max = int.MaxValue)
        {
            int result;

            if (!strCache.ContainsKey(key))
            {
                strCache[key] = num.ToString();
            }

            // 如果有规定宽度，则使用规定的宽度
            if (width > 0)
            {
                strCache[key] = GUILayout.TextField(strCache[key], GUILayout.Width(width));
            }
            else
            {
                strCache[key] = GUILayout.TextField(strCache[key]);
            }
            // 如果缓存中的数据可以解析，则返回解析的数据并删除缓存数据
            if (int.TryParse(strCache[key], out result))
            {
                strCache.Remove(key);
                result = Mathf.Clamp(result, min, max);
                return result;
            }
            else
            {
                return num;
            }
        }

        /// <summary>
        /// 整形数据的输入GUI
        /// </summary>
        /// <returns></returns>
        public static uint UIntTextGUI(uint num, string key, int width = 0, uint min = uint.MinValue, uint max = uint.MaxValue)
        {
            uint result;

            if (!strCache.ContainsKey(key))
            {
                strCache[key] = num.ToString();
            }

            // 如果有规定宽度，则使用规定的宽度
            if (width > 0)
            {
                strCache[key] = GUILayout.TextField(strCache[key], GUILayout.Width(width));
            }
            else
            {
                strCache[key] = GUILayout.TextField(strCache[key]);
            }
            // 如果缓存中的数据可以解析，则返回解析的数据并删除缓存数据
            if (uint.TryParse(strCache[key], out result))
            {
                strCache.Remove(key);
                if (result < min) result = min;
                if (result > max) result = max;
                return result;
            }
            else
            {
                return num;
            }
        }

        /// <summary>
        /// 长整形数据的输入GUI
        /// </summary>
        /// <returns></returns>
        public static long LongTextGUI(long num, string key, int width = 0, long min = long.MinValue, long max = long.MaxValue)
        {
            long result;

            if (!strCache.ContainsKey(key))
            {
                strCache[key] = num.ToString();
            }

            // 如果有规定宽度，则使用规定的宽度
            if (width > 0)
            {
                strCache[key] = GUILayout.TextField(strCache[key], GUILayout.Width(width));
            }
            else
            {
                strCache[key] = GUILayout.TextField(strCache[key]);
            }
            // 如果缓存中的数据可以解析，则返回解析的数据并删除缓存数据
            if (long.TryParse(strCache[key], out result))
            {
                strCache.Remove(key);
                if (result < min) result = min;
                if (result > max) result = max;
                return result;
            }
            else
            {
                return num;
            }
        }

        /// <summary>
        /// 长整形数据的输入GUI
        /// </summary>
        /// <returns></returns>
        public static ulong ULongTextGUI(ulong num, string key, int width = 0, ulong min = ulong.MinValue, ulong max = ulong.MaxValue)
        {
            ulong result;

            if (!strCache.ContainsKey(key))
            {
                strCache[key] = num.ToString();
            }

            // 如果有规定宽度，则使用规定的宽度
            if (width > 0)
            {
                strCache[key] = GUILayout.TextField(strCache[key], GUILayout.Width(width));
            }
            else
            {
                strCache[key] = GUILayout.TextField(strCache[key]);
            }
            // 如果缓存中的数据可以解析，则返回解析的数据并删除缓存数据
            if (ulong.TryParse(strCache[key], out result))
            {
                strCache.Remove(key);
                if (result < min) result = min;
                if (result > max) result = max;
                return result;
            }
            else
            {
                return num;
            }
        }

        /// <summary>
        /// 整形数据的输入GUI
        /// </summary>
        /// <returns></returns>
        public static float FloatTextGUI(float num, string key, int width = 0, float min = float.MinValue, float max = float.MaxValue)
        {
            float result;

            if (!strCache.ContainsKey(key))
            {
                strCache[key] = num.ToString();
            }

            // 如果有规定宽度，则使用规定的宽度
            if (width > 0)
            {
                strCache[key] = GUILayout.TextField(strCache[key], GUILayout.Width(width));
            }
            else
            {
                strCache[key] = GUILayout.TextField(strCache[key]);
            }
            // 如果缓存中的数据可以解析，则返回解析的数据并删除缓存数据
            if (float.TryParse(strCache[key], out result))
            {
                strCache.Remove(key);
                result = Mathf.Clamp(result, min, max);
                return result;
            }
            else
            {
                return num;
            }
        }
    }
}
