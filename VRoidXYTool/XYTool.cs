using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio;
using BepInEx.Configuration;
using System.IO;
using VRoidStudio.GUI.AvatarEditor.PhotoBooth;
using VRoidStudio.GUI.AvatarEditor;

namespace VRoidXYTool
{
    [BepInPlugin(PluginID, PluginName, PluginVersion)]
    public partial class XYTool : BaseUnityPlugin
    {
        public const string PluginID = "me.xiaoye97.plugin.VRoidStudio.VRoidXYTool";
        public const string PluginName = "VRoidXYTool";
        public const string PluginVersion = "0.7.0";

        public static XYTool Inst;

        #region 工具
        public CameraTool CameraTool;
        public GuideTool GuideTool;
        public LinkTextureTool LinkTextureTool;
        public PosePersetTool PosePersetTool;
        public MMDTool MMDTool;
        public VideoTool VideoTool;
        #endregion

        #region 引用
        public AvatarEditor AvatarEditor;
        public MainViewModel MainVM
        {
            get
            {
                if (AvatarEditor != null)
                {
                    return AvatarEditor._viewModel;
                }
                return null;
            }
        }
        public PhotoBoothViewModel PhotoBoothVM
        {
            get
            {
                if (AvatarEditor != null)
                {
                    return AvatarEditor._instantiatedPhotoBoothViewModel;
                }
                return null;
            }
        }

        public CurrentFileModel CurrentFileM
        {
            get
            {
                if (CurrentFileVM != null)
                {
                    return CurrentFileVM.model;
                }
                return null;
            }
        }

        public CurrentFileViewModel CurrentFileVM
        {
            get
            {
                if (MainVM != null)
                {
                    return MainVM.CurrentFile;
                }
                return null;
            }
        }

        /// <summary>
        /// 模型是否为空
        /// </summary>
        public bool IsModelNull
        {
            get
            {
                if (CurrentFileVM == null) return true;
                if (CurrentFileM == null) return true;
                return false;
            }
        }

        /// <summary>
        /// 当前模型的名字，如果还未保存，则返回null
        /// </summary>
        public string CurrentModelName
        {
            get
            {
                if (XYTool.Inst.IsModelNull) return null;
                // 获取模型名字
                string modelPath = XYTool.Inst.CurrentFileM.path;
                if (string.IsNullOrWhiteSpace(modelPath)) return null;
                FileInfo modelFile = new FileInfo(modelPath);
                if (!modelFile.Exists) return null;
                string modelName = modelFile.Name.Replace(".vroid", "");
                return modelName;
            }
        }
        #endregion

        #region 配置
        public ConfigEntry<SystemLanguage> PluginLanguage;
        public ConfigEntry<bool> RunInBG;
        public ConfigEntry<KeyCode> GUIHotkey;
        public ConfigEntry<KeyCode> MiniGUIHotkey;
        #endregion

        private void Awake()
        {
            Inst = this;
            // 多语言
            I18N.Init();
            PluginLanguage = Config.Bind<SystemLanguage>("Common", "Language", Application.systemLanguage, "Plugin language");
            I18N.SetLanguage(PluginLanguage.Value);
            // 绑定配置
            RunInBG = Config.Bind<bool>("Common", "RunInBG", true, "RunInBGDesc".Translate());
            GUIHotkey = Config.Bind<KeyCode>("Common", "GUIHotkey", KeyCode.Tab, "GUIHotkey".Translate());
            MiniGUIHotkey = Config.Bind<KeyCode>("Common", "MiniGUIHotkey", KeyCode.BackQuote, "MiniGUIHotkey".Translate());

            Logger.LogInfo("XYTool启动");
        }

        private void Start()
        {
            AvatarEditor = GameObject.FindObjectOfType<AvatarEditor>();
            // Patch
            Harmony.CreateAndPatchAll(typeof(XYTool));
            CameraTool = new CameraTool();
            GuideTool = new GuideTool();
            LinkTextureTool = new LinkTextureTool();
            PosePersetTool = new PosePersetTool();
            MMDTool = new MMDTool();
            VideoTool = new VideoTool();
            // UI窗口
            InitWindow();
            headTex = XYModLib.ResourceUtils.GetTex("head_xiaoye.png");
        }

        private void Update()
        {
            // 控制界面显示
            if (Input.GetKeyDown(GUIHotkey.Value))
            {
                Window.Show = !Window.Show;
            }
            if (Input.GetKeyDown(MiniGUIHotkey.Value))
            {
                MiniWindow.Show = !MiniWindow.Show;
            }
            // 控制配置中的值同步
            if (Application.runInBackground != RunInBG.Value)
            {
                Application.runInBackground = RunInBG.Value;
            }
            // 工具的Update
            CameraTool.Update();
            LinkTextureTool.Update();
            VideoTool.Update();
        }

        /// <summary>
        /// 切换到主界面时，清理各种数据
        /// </summary>
        [HarmonyPostfix, HarmonyPatch(typeof(VRoid.Studio.StartScreen.ViewModel), MethodType.Constructor, new Type[] { typeof(VRoid.Studio.MainViewModel), typeof(VRoid.UI.BindableResources), typeof(VRoid.Studio.GlobalBus) })]
        public static void StartScreenPatch()
        {
            if (Inst.LinkTextureTool != null)
            {
                Inst.LinkTextureTool.Clear();
            }
        }
    }
}