using System;
using BepInEx;
using VRoid.UI;
using System.IO;
using HarmonyLib;
using System.Linq;
using UnityEngine;
using VRoid.Studio;
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

        private CurrentFileModel _CurrentFileM;

        public CurrentFileModel CurrentFileM
        {
            get
            {
                if (_CurrentFileM == null )
                {
                    if (CurrentFileVM != null)
                    {
                        _CurrentFileM = CurrentFileVM.model;
                    }
                }
                return  _CurrentFileM;
            }
        }

        private CurrentFileViewModel _CurrentFileVM;

        public CurrentFileViewModel CurrentFileVM
        {
            get
            {
                if (_CurrentFileVM == null)
                {
                    if (XYTool.Inst.MainVM != null)
                    {
                        _CurrentFileVM = XYTool.Inst.MainVM.CurrentFile;
                    }
                }
                return _CurrentFileVM;
            }
        }

        public ConfigEntry<string> LinkTextureDirectory;
        public ConfigEntry<float> LinkTextureSyncInterval;
        public ConfigEntry<bool> UseBaseDir;

        public LinkTextureTool()
        {
            Harmony.CreateAndPatchAll(typeof(LinkTextureTool));
            LinkTextures = new List<LinkTexture>();
            // 链接纹理路径
            LinkTextureDirectory = XYTool.Inst.Config.Bind<string>("LinkTextureTool", "LinkTextureDirectory", "", "LinkTextureDirectoryDesc".Translate());
            UseBaseDir = XYTool.Inst.Config.Bind<bool>("LinkTextureTool", "UseBaseDirectory", false, "UseBaseDirectoryDesc".Translate());
            LinkTextureSyncInterval = XYTool.Inst.Config.Bind<float>("LinkTextureTool", "LinkTextureSyncInterval", 0.2f, "LinkTextureSyncIntervalDesc".Translate());
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
            GUILayout.BeginVertical("LinkTextureTool".Translate(), GUI.skin.window);
            try
            {
                // 显示链接文件夹
                if (useConfigDir)
                {
                    if (linkDir != null && linkDir.Exists)
                    {
                        GUILayout.Label(string.Format("NowUseLinkTextureDir".Translate(), linkDir.FullName));
                    }
                    else
                    {
                        GUILayout.Label(string.Format("NowUseLinkTextureDir".Translate(), baseDir.FullName));
                    }
                }
                if (GUILayout.Button("OpenLinkTextureDir".Translate()))
                {
                    if (IsModelNull())
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
                                GUILayout.Label("HasDuplicateNameCantExport".Translate());
                                if (GUILayout.Button("RandomName".Translate()))
                                {
                                    RandomTextureName(lt);
                                }
                            }
                            else
                            {
                                if (GUILayout.Button("ImportNow".Translate()))
                                {
                                    ImportTexture(lt, false);
                                }
                                if (GUILayout.Button("ExportTexture".Translate()))
                                {
                                    ExportTexture(lt);
                                }
                                if (lt.CanExportUV)
                                {
                                    if (GUILayout.Button("ExportGuide".Translate()))
                                    {
                                        ExportUV(lt);
                                    }
                                }
                            }

                            //GUILayout.Label(lt.LastWriteTime.ToString());
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label("NowNotEditingTexture".Translate());
                }
            }
            catch (Exception e)
            {
                GUILayout.Label($"Exception:{e.Message}\n{e.StackTrace}");
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
        /// 模型是否为空
        /// </summary>
        private bool IsModelNull()
        {
            if (CurrentFileVM == null) return true;
            if (CurrentFileM == null) return true;
            return false;
        }

        /// <summary>
        /// 检查是否可以使用链接纹理工具
        /// </summary>
        /// <returns></returns>
        private bool CanUseTool()
        {
            if (IsModelNull()) return false;
            // 获取模型名字
            string modelPath = CurrentFileM.path;
            if (string.IsNullOrWhiteSpace(modelPath)) return false;
            FileInfo modelFile = new FileInfo(modelPath);
            if (!modelFile.Exists) return false;
            string modelName = modelFile.Name.Replace(".vroid", "");
            if (UseBaseDir.Value)
            {
                linkDir = baseDir;
            }
            else
            {
                linkDir = new DirectoryInfo($"{baseDir.FullName}/{modelName}");
            }
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
                    GetRasterLayerContentQuery.Result result = CurrentFileM.engine.Context.ExecuteSyncQuery<GetRasterLayerContentQuery.Result>(new GetRasterLayerContentQuery(lt.layer.Path));
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
                var bytes = await CurrentFileVM.Engine.GetUVGuideTexturePNGBytes(referringTexturePath);
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
                        ImportTexture(lt, true);
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
        /// 导入纹理
        /// </summary>
        /// <param name="lt"></param>
        /// <param name="checkTime">是否检查时间</param>
        private void ImportTexture(LinkTexture lt, bool checkTime)
        {
            FileInfo texFile = new FileInfo($"{linkDir}/{lt.layer.TranslatedDisplayName}.png");
            // 检查是否有一致名字的纹理
            if (texFile.Exists)
            {
                // 如果文件的最后写入时间比记录的要新，则同步纹理
                if (!checkTime || texFile.LastWriteTime > lt.LastWriteTime)
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
                        return;
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
                    CurrentFileM.engine.Context.ExecuteSyncCommand(new LoadImageToEditableImageRasterLayerCommand(lt.layer.Path, bitmapSize, pixels));
                    lt.LastWriteTime = DateTime.Now;
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
            CurrentFileM.engine.Context.ExecuteSyncCommand(ActionHandler.CreateModifyNameCommand(lt.layer.Path, name));
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
