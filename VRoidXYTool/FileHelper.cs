using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using VRoid.Studio.Util;
using System.Reflection;

namespace VRoidXYTool
{
    public static class FileHelper
    {
        /// <summary>
        /// 加载ab包内容
        /// </summary>
        public static T LoadAsset<T>(string abName, string assetName) where T : UnityEngine.Object
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"VRoidXYTool.{abName}");
            var ab = AssetBundle.LoadFromStream(stream);
            T result = ab.LoadAsset<T>(assetName);
            ab.Unload(false);
            return result;
        }

        /// <summary>
        /// 加载外部纹理
        /// </summary>
        public static Texture2D LoadTexture2D(string path)
        {
            byte[] fileBytes;
            try
            {
                fileBytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                if (!(e is IOException))
                {
                    Debug.LogError($"读取纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                }
                return null;
            }
            Texture2D texture2D;
            try
            {
                texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
                if (!texture2D.LoadImage(fileBytes))
                {
                    throw new Exception("加载图片异常");
                }
            }
            catch
            {
                return null;
            }
            return texture2D;
        }

        /// <summary>
        /// 加载外部纹理并转换为精灵
        /// </summary>
        public static Sprite LoadSprite(string path)
        {
            Texture2D texture2D = LoadTexture2D(path);
            if (texture2D == null)
            {
                return null;
            }
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        /// <summary>
        /// 加载json并转换为类对象
        /// </summary>
        public static T LoadJson<T>(string path) where T : class
        {
            try
            {
                if (File.Exists(path))
                {
                    var jsonStr = File.ReadAllText(path);
                    T result = JsonConvert.DeserializeObject<T>(jsonStr);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取Json时出现异常:\n{e.Message}\n{e.StackTrace}");
            }
            return null;
        }

        /// <summary>
        /// 保存json到文件
        /// </summary>
        public static void SaveJson<T>(string path, T data, Formatting format = Formatting.Indented)
        {
            try
            {
                FileInfo file = new FileInfo(path);
                if (!file.Directory.Exists)
                {
                    file.Directory.Create();
                }
                string json = JsonConvert.SerializeObject(data, format);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"保存Json时出现异常:\n{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 图片的后缀名
        /// </summary>
        public static FileDialogCommonUtil.ExtensionFilter[] GetImageFilters()
        {
            return new FileDialogCommonUtil.ExtensionFilter[]
            {
                new FileDialogCommonUtil.ExtensionFilter("Common.Image".Translate(), new string[]
                {
                    "png",
                    "jpg"
                })
            };
        }

        /// <summary>
        /// json的后缀名
        /// </summary>
        public static FileDialogCommonUtil.ExtensionFilter[] GetJsonFilters()
        {
            return new FileDialogCommonUtil.ExtensionFilter[]
            {
                GetJsonFilter()
            };
        }

        public static FileDialogCommonUtil.ExtensionFilter GetJsonFilter()
        {
            return new FileDialogCommonUtil.ExtensionFilter("Json", new string[]
                {
                    "json"
                });
        }

        /// <summary>
        /// 姿势json的后缀名
        /// </summary>
        public static FileDialogCommonUtil.ExtensionFilter[] GetPoseJsonFilters()
        {
            return new FileDialogCommonUtil.ExtensionFilter[]
            {
                new FileDialogCommonUtil.ExtensionFilter("PoseJson", new string[]
                {
                    "posejson"
                })
            };
        }

        /// <summary>
        /// MMD动作的后缀名
        /// </summary>
        public static FileDialogCommonUtil.ExtensionFilter[] GetVMDFilters()
        {
            return new FileDialogCommonUtil.ExtensionFilter[]
            {
                new FileDialogCommonUtil.ExtensionFilter("Vocaloid Motion Data", new string[]
                {
                    "vmd"
                })
            };
        }
    }
}