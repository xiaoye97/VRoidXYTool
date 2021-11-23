using System;
using System.IO;
using UnityEngine;
using VRoid.Studio.Util;
using System.Reflection;

namespace VRoidXYTool
{
    public static class FileHelper
    {
        /// <summary>
        /// 加载ab包内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// <param name="path"></param>
        /// <returns></returns>
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
        /// 图片的后缀名
        /// </summary>
        /// <returns></returns>
        public static FileDialogUtil.ExtensionFilter[] GeImageFilters()
        {
            return new FileDialogUtil.ExtensionFilter[]
            {
                new FileDialogUtil.ExtensionFilter("图片", new string[]
                {
                    "png",
                    "jpg"
                })
            };
        }
    }
}
