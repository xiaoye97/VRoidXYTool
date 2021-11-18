using System;
using BepInEx;
using VRoid.UI;
using System.IO;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio.Util;
using VRoidCore.Editing.Query;
using System.Collections.Generic;
using VRoidCore.Draw.TiledBitmap;
using VRoidCore.Common.SpecificTypes;
using VRoidCore.Editing.History.Command;
using VRoid.Studio.TextureEditor.Layer.ViewModel;
using VRoid.Studio.TextureEditor.Texture.ViewModel;

namespace VRoidXYTool
{
    public class LinkTextureTool
    {
        private static List<LinkTexture> LinkTextures;
        private Vector2 svPos;

        private float refreshCD;

        public LinkTextureTool()
        {
            Harmony.CreateAndPatchAll(typeof(LinkTextureTool));
            LinkTextures = new List<LinkTexture>();
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical("纹理链接工具", GUI.skin.window);
            if (LinkTextures.Count > 0)
            {
                if (GUILayout.Button("一键导出"))
                {
                    OnClickExportLinkTexture();
                }
                svPos = GUILayout.BeginScrollView(svPos, GUI.skin.box, GUILayout.MaxHeight(200));
                foreach (var lt in LinkTextures)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{lt.layer.TranslatedDisplayName}");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(lt.LastWriteTime.ToString());
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("没有正在编辑的纹理");
            }
            GUILayout.EndVertical();
        }

        public void Update()
        {
            if (refreshCD < 0)
            {
                refreshCD = 1;
                RefreshAllLayer();
            }
            else
            {
                refreshCD -= Time.deltaTime;
            }
        }

        /// <summary>
        /// 一键导出链接纹理
        /// </summary>
        public void OnClickExportLinkTexture()
        {
            if (XYTool.Inst.CurrentModelFile == null) return;
            // 获取模型名字
            string modelPath = XYTool.Inst.CurrentModelFile.path;
            if (string.IsNullOrWhiteSpace(modelPath)) return;
            FileInfo modelFile = new FileInfo(modelPath);
            if (!modelFile.Exists) return;
            string modelName = modelFile.Name.Replace(".vroid", "");
            //Debug.Log($"模型名字:{modelName}");
            DirectoryInfo linkDir = new DirectoryInfo($"{Paths.GameRootPath}/LinkTexture/{modelName}");
            if (!linkDir.Exists)
            {
                linkDir.Create();
            }
            int count = LinkTextures.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var lt = LinkTextures[i];
                if (lt.layer != null)
                {
                    GetRasterLayerContentQuery.Result result = XYTool.Inst.CurrentModelFile.engine.Context.ExecuteSyncQuery<GetRasterLayerContentQuery.Result>(new GetRasterLayerContentQuery(lt.layer.Path));
                    byte[] bytes;
                    try
                    {
                        bytes = ImageEncodingUtil.EncodeToPNG(result.size, result.rgbaBytes);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"转换纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                        return;
                    }
                    try
                    {
                        string path = $"{linkDir}/{lt.layer.TranslatedDisplayName}.png";
                        File.WriteAllBytes(path, bytes);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"导出纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// 刷新当前所有存储的层
        /// </summary>
        public void RefreshAllLayer()
        {
            if (XYTool.Inst.CurrentModelFile == null) return;
            // 获取模型名字
            string modelPath = XYTool.Inst.CurrentModelFile.path;
            if (string.IsNullOrWhiteSpace(modelPath)) return;

            FileInfo modelFile = new FileInfo(modelPath);
            if (!modelFile.Exists) return;
            string modelName = modelFile.Name.Replace(".vroid", "");
            //Debug.Log($"模型名字:{modelName}");
            DirectoryInfo linkDir = new DirectoryInfo($"{Paths.GameRootPath}/LinkTexture/{modelName}");
            if (!linkDir.Exists)
            {
                linkDir.Create();
            }
            // 遍历刷新
            int count = LinkTextures.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var lt = LinkTextures[i];
                if (lt.layer != null)
                {
                    FileInfo texFile = new FileInfo($"{linkDir}/{lt.layer.TranslatedDisplayName}.png");
                    // 检查是否有一致名字的纹理
                    if (texFile.Exists)
                    {
                        // 如果文件的最后写入时间比记录的要新，则同步纹理
                        if (texFile.LastWriteTime > lt.LastWriteTime)
                        {
                            byte[] fileBytes;
                            try
                            {
                                fileBytes = File.ReadAllBytes(texFile.FullName);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"读取纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                                continue;
                            }
                            BitmapSize bitmapSize;
                            UnityEngine.Color[] pixels;
                            try
                            {
                                Decode(fileBytes, out bitmapSize, out pixels);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"解析纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                                return;
                            }
                            XYTool.Inst.CurrentModelFile.engine.Context.ExecuteSyncCommand(new LoadImageToEditableImageRasterLayerCommand(lt.layer.Path, bitmapSize, pixels));
                            lt.LastWriteTime = DateTime.Now;
                        }
                    }
                }
                else
                {
                    LinkTextures.RemoveAt(i);
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RasterLayerViewModel), MethodType.Constructor, new Type[] { typeof(BindableResources), typeof(VRoid.Studio.Engine.Model), typeof(VRoid.Studio.TextureEditor.ViewModel), typeof(TextureViewModel), typeof(EditableImageRasterLayerPath) })]
        public static void RasterLayerVMPatch(RasterLayerViewModel __instance)
        {
            Debug.Log($"构造了RasterLayerViewModel, Name:{__instance.TranslatedDisplayName}");
            LinkTextures.Add(new LinkTexture(__instance));
        }

        public static void Decode(byte[] fileBytes, out BitmapSize bitmapSize, out UnityEngine.Color[] pixels)
        {
            Texture2D texture2D = null;
            try
            {
                texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
                if (!texture2D.LoadImage(fileBytes))
                {
                    throw new Exception("加载图片异常");
                }
                bitmapSize = new BitmapSize(texture2D.width, texture2D.height);
                pixels = texture2D.GetPixels();
            }
            finally
            {
                if (texture2D != null)
                {
                    UnityEngine.Object.Destroy(texture2D);
                }
            }
        }
    }

    public class LinkTexture
    {
        public RasterLayerViewModel layer;
        public DateTime LastWriteTime;

        public LinkTexture(RasterLayerViewModel vm)
        {
            layer = vm;
            LastWriteTime = DateTime.Now;
        }
    }
}
