using System;
using BepInEx;
using VRoid.UI;
using System.IO;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using VRoid.Studio.Util;
using BepInEx.Configuration;
using VRoidCore.Editing.Query;
using System.Collections.Generic;
using VRoid.Studio.TextureEditor;
using VRoidCore.Draw.TiledBitmap;
using VRoidCore.Common.SpecificTypes;
using VRoidCore.Editing.History.Command;
using VRoid.Studio.TextureEditor.Layer.ViewModel;
using VRoid.Studio.TextureEditor.Texture.ViewModel;

namespace VRoidXYTool
{
    public class LinkTextureTool
    {
        public static List<LinkTexture> LinkTextures;
        private Vector2 svPos;

        private float refreshCD;

        /// <summary>
        /// 基础工作目录
        /// </summary>
        private DirectoryInfo baseDir;

        /// <summary>
        /// 最终链接文件夹
        /// </summary>
        private DirectoryInfo linkDir;

        private bool useConfigDir;

        public ConfigEntry<string> LinkTextureDirectory;

        public ConfigEntry<float> LinkTextureSyncInterval;

        public LinkTextureTool()
        {
            Harmony.CreateAndPatchAll(typeof(LinkTextureTool));
            LinkTextures = new List<LinkTexture>();
            // 链接纹理路径
            LinkTextureDirectory = XYTool.Inst.Config.Bind<string>("LinkTextureTool", "LinkTextureDirectory", "", "自定义的链接纹理检测路径，留空则使用默认路径");
            LinkTextureSyncInterval = XYTool.Inst.Config.Bind<float>("LinkTextureTool", "LinkTextureSyncInterval", 1f, "纹理同步检测的间隔时间，单位秒，最低0.1秒");
            LinkTextureSyncInterval.Value = Mathf.Max(0.1f, LinkTextureSyncInterval.Value);
            useConfigDir = false;
            if (!string.IsNullOrWhiteSpace(LinkTextureDirectory.Value))
            {
                baseDir = new DirectoryInfo(LinkTextureDirectory.Value);
                if (baseDir.Exists)
                {
                    useConfigDir = true;
                }
                else
                {
                    Debug.LogWarning($"自定义链接纹理路径不存在，使用默认路径");
                }
            }
            if (!useConfigDir)
            {
                baseDir = new DirectoryInfo($"{Paths.GameRootPath}/LinkTexture");
            }
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical("纹理链接工具", GUI.skin.window);
            try
            {
                // 显示链接文件夹
                if (useConfigDir)
                {
                    if (linkDir != null && linkDir.Exists)
                    {
                        GUILayout.Label($"正在使用自定义链接文件夹:{linkDir.FullName}");
                    }
                    else
                    {
                        GUILayout.Label($"正在使用自定义链接文件夹:{baseDir.FullName}");
                    }
                }
                if (GUILayout.Button("打开链接纹理文件夹"))
                {
                    if (XYTool.Inst.CurrentModelFile != null)
                    {
                        if (linkDir != null && linkDir.Exists)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", linkDir.FullName);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("explorer.exe", baseDir.FullName);
                    }
                }
                if (LinkTextures.Count > 0)
                {
                    if (GUILayout.Button("全部导出"))
                    {
                        OnClickExportLinkTexture();
                    }
                    svPos = GUILayout.BeginScrollView(svPos, GUI.skin.box, GUILayout.MaxHeight(200));

                    for (int i = 0; i < LinkTextures.Count; i++)
                    {
                        var lt = LinkTextures[i];
                        if (lt != null && lt.IsVaild())
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"{lt.layer.TranslatedDisplayName}");
                            GUILayout.FlexibleSpace();
                            if (HasDuplicateName(lt))
                            {
                                GUILayout.Label("有重名图层，无法导出!");
                                if (GUILayout.Button("随机名字"))
                                {
                                    RandomTextureName(lt);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("导出纹理"))
                                {
                                    ExportTexture(lt);
                                }
                                if (lt.CanExportUV)
                                {
                                    if (GUILayout.Button("导出UV"))
                                    {
                                        ExportUV(lt);
                                    }
                                }
                            }

                            GUILayout.Label(lt.LastWriteTime.ToString());
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label("没有正在编辑的纹理");
                }
            }
            catch (Exception e)
            {
                GUILayout.Label($"出现异常:{e.Message}\n{e.StackTrace}");
            }
            GUILayout.EndVertical();
        }

        public void Update()
        {
            if (refreshCD < 0)
            {
                refreshCD = LinkTextureSyncInterval.Value;
                RefreshAllLayer();
            }
            else
            {
                refreshCD -= Time.deltaTime;
            }
        }

        /// <summary>
        /// 检查是否可以使用链接纹理工具
        /// </summary>
        /// <returns></returns>
        private bool CanUseTool()
        {
            if (XYTool.Inst.CurrentModelFile == null) return false;
            // 获取模型名字
            string modelPath = XYTool.Inst.CurrentModelFile.path;
            if (string.IsNullOrWhiteSpace(modelPath)) return false;
            FileInfo modelFile = new FileInfo(modelPath);
            if (!modelFile.Exists) return false;
            string modelName = modelFile.Name.Replace(".vroid", "");
            linkDir = new DirectoryInfo($"{baseDir.FullName}/{modelName}");
            if (!linkDir.Exists)
            {
                linkDir.Create();
            }
            return true;
        }

        /// <summary>
        /// 导出链接纹理
        /// </summary>
        /// <param name="lt"></param>
        public void ExportTexture(LinkTexture lt)
        {
            if (CanUseTool())
            {
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
        /// 导出UV
        /// </summary>
        /// <param name="lt"></param>
        public async void ExportUV(LinkTexture lt)
        {
            try
            {
                TexturePath referringTexturePath = lt.layer._parent.ReferringTexturePaths.FirstOrDefault<TexturePath>();
                var bytes = await XYTool.Inst.CurrentViewModelFile.Engine.GetUVGuideTexturePNGBytes(referringTexturePath);
                string path = $"{linkDir}/{lt.layer.TranslatedDisplayName}_UV.png";
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                Debug.LogError($"导出UV时出现异常:\n{e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 一键导出链接纹理
        /// </summary>
        public void OnClickExportLinkTexture()
        {
            if (CanUseTool())
            {
                int count = LinkTextures.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    var lt = LinkTextures[i];
                    if (lt != null && lt.IsVaild() && !HasDuplicateName(lt))
                    {
                        ExportTexture(lt);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新当前所有存储的层
        /// </summary>
        public void RefreshAllLayer()
        {
            if (CanUseTool())
            {
                // 遍历刷新
                int count = LinkTextures.Count;
                for (int i = count - 1; i >= 0; i--)
                {
                    var lt = LinkTextures[i];
                    if (lt != null && lt.IsVaild())
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
                                    if (!(e is IOException))
                                    {
                                        Debug.LogError($"读取纹理时出现异常:\n{e.Message}\n{e.StackTrace}");
                                    }
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
            else
            {
                if (LinkTextures.Count > 0)
                {
                    LinkTextures.Clear();
                }
            }
        }

        /// <summary>
        /// 检查LinkTexture是否重名
        /// </summary>
        public bool HasDuplicateName(LinkTexture target)
        {
            foreach (var lt in LinkTextures)
            {
                if (lt != target && lt.IsVaild())
                {
                    if (lt.layer.TranslatedDisplayName == target.layer.TranslatedDisplayName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            LinkTextures.Clear();
            linkDir = null;
        }

        /// <summary>
        /// 随机给纹理生成名字
        /// </summary>
        /// <param name="lt"></param>
        public void RandomTextureName(LinkTexture lt)
        {
            string name = GetRandomName();
            XYTool.Inst.CurrentModelFile.engine.Context.ExecuteSyncCommand(ActionHandler.CreateModifyNameCommand(lt.layer.Path, name));
        }

        /// <summary>
        /// 随机一个图层名字
        /// </summary>
        /// <returns></returns>
        public string GetRandomName()
        {
            int r = UnityEngine.Random.Range(1000, 10000);
            return $"layer_{r}";
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RasterLayerViewModel), MethodType.Constructor, new Type[] { typeof(BindableResources), typeof(VRoid.Studio.Engine.Model), typeof(VRoid.Studio.TextureEditor.ViewModel), typeof(TextureViewModel), typeof(EditableImageRasterLayerPath) })]
        public static void RasterLayerVMPatch(RasterLayerViewModel __instance)
        {
            Debug.Log($"构造了RasterLayerViewModel, Name:{__instance.TranslatedDisplayName}");
            // 检查当前存储的链接纹理，是否有相同纹理
            for (int i = 0; i < LinkTextures.Count; i++)
            {
                var lt = LinkTextures[i];
                if (lt.layer.Path.NodeId == __instance.Path.NodeId)
                {
                    //Debug.Log($"新构造的RasterLayerViewModel有相同ID在存储内，替换");
                    LinkTextures[i] = new LinkTexture(__instance);
                    return;
                }
            }
            LinkTextures.Add(new LinkTexture(__instance));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VRoid.Studio.StartScreen.ViewModel), MethodType.Constructor, new Type[] { typeof(VRoid.Studio.MainViewModel), typeof(BindableResources), typeof(VRoid.Studio.GlobalBus) })]
        public static void StartScreenPatch()
        {
            XYTool.Inst.LinkTextureTool.Clear();
        }

        /// <summary>
        /// 解码图片
        /// </summary>
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

    /// <summary>
    /// 链接纹理数据
    /// </summary>
    public class LinkTexture
    {
        public RasterLayerViewModel layer;
        public DateTime LastWriteTime;
        public bool CanExportUV;

        public LinkTexture(RasterLayerViewModel vm)
        {
            layer = vm;
            LastWriteTime = DateTime.Now;
            TexturePath referringTexturePath = vm._parent.ReferringTexturePaths.FirstOrDefault<TexturePath>();
            if (referringTexturePath != null && vm.featureViewModel.actionHandler.IsGuideExportable(referringTexturePath))
            {
                CanExportUV = true;
            }
        }

        /// <summary>
        /// 纹理是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsVaild()
        {
            if (layer == null)
            {
                return false;
            }
            try
            {
                string name = layer.TranslatedDisplayName;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
