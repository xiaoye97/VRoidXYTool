using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio;
using BepInEx.Configuration;

namespace VRoidXYTool
{
    [BepInPlugin(PluginID, PluginName, PluginVersion)]
    public class XYTool : BaseUnityPlugin
    {
        public const string PluginID = "me.xiaoye97.plugin.VRoidStudio.VRoidXYTool";
        public const string PluginName = "VRoidXYTool";
        public const string PluginVersion = "0.3.5";

        private bool showWindow;
        public bool ShowWindow
        {
            get { return showWindow; }
            set 
            {
                if (showWindow != value)
                {
                    showWindow = value;
                    if (showWindow)
                    {
                        RayBlocker.OpenBlocker();
                    }
                    else
                    {
                        RayBlocker.CloseBlocker();
                    }
                }
            }
        }
        public Rect ToolWindowRect = new Rect(50, 50, 500, 600);

        public static XYTool Inst;

        public MainViewModel MainVM;

        public CameraTool CameraTool;
        public GuideTool GuideTool;
        public LinkTextureTool LinkTextureTool;
        public RayBlocker RayBlocker;

        #region 配置
        public ConfigEntry<SystemLanguage> PluginLanguage;
        public ConfigEntry<bool> RunInBG;
        public ConfigEntry<KeyCode> Hotkey;

        public ConfigEntry<bool> ShowCameraToolGUI;
        public ConfigEntry<bool> ShowGuideToolGUI;
        public ConfigEntry<bool> ShowLinkTextureToolGUI;
        #endregion

        private void Awake()
        {
            Inst = this;
            I18N.Init();
            PluginLanguage = Config.Bind<SystemLanguage>("Common", "Language", Application.systemLanguage, "Plugin language");
            I18N.SetLanguage(PluginLanguage.Value);

            RunInBG = Config.Bind<bool>("Common", "RunInBG", true, "RunInBGDesc".Translate());
            Hotkey = Config.Bind<KeyCode>("Common", "Hotkey", KeyCode.F11, "GUIHotkey".Translate());
            ShowCameraToolGUI = Config.Bind<bool>("Common", "ShowCameraToolGUI", true, "ShowCameraToolGUI".Translate());
            ShowGuideToolGUI = Config.Bind<bool>("Common", "ShowGizmoToolGUI", true, "ShowGuideToolGUI".Translate());
            ShowLinkTextureToolGUI = Config.Bind<bool>("Common", "ShowLinkTextureToolGUI", true, "ShowLinkTextureToolGUI".Translate());
            Harmony.CreateAndPatchAll(typeof(XYTool));
            Logger.LogInfo("XYTool启动");
        }

        private void Start()
        {
            CameraTool = new CameraTool();
            GuideTool = new GuideTool();
            LinkTextureTool = new LinkTextureTool();
            RayBlocker = new RayBlocker();
        }

        private void Update()
        {
            // 控制界面显示
            if (Input.GetKeyDown(Hotkey.Value))
            {
                ShowWindow = !ShowWindow;
            }
            // 控制配置中的值同步
            if (Application.runInBackground != RunInBG.Value)
            {
                Application.runInBackground = RunInBG.Value;
            }
            // 工具的Update
            CameraTool.Update();
            LinkTextureTool.Update();
            if (ShowWindow)
            {
                RayBlocker.Update();
            }
        }

        private void OnGUI()
        {
            if (ShowWindow)
            {
                ToolWindowRect = GUILayout.Window(666, ToolWindowRect, WindowFunc, string.Format("PluginWindowTitle".Translate(), PluginVersion), GUI.skin.box);
            }
        }

        /// <summary>
        /// 小工具主界面
        /// </summary>
        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                ShowWindow = false;
            }
            GUILayout.EndHorizontal();
            InfoGUI();
            GUILayout.Space(3);
            ConfigGUI();
            if (ShowCameraToolGUI.Value)
            {
                GUILayout.Space(3);
                CameraTool.OnGUI();
            }
            if (ShowGuideToolGUI.Value)
            {
                GUILayout.Space(3);
                GuideTool.OnGUI();
            }
            if (ShowLinkTextureToolGUI.Value)
            {
                GUILayout.Space(3);
                LinkTextureTool.OnGUI();
            }
            GUI.DragWindow();
        }

        /// <summary>
        /// 关于界面
        /// </summary>
        public void InfoGUI()
        {
            GUILayout.BeginVertical("Info".Translate(), GUI.skin.window);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author".Translate());
            GUILayout.Space(10);
            if (GUILayout.Button("TutorialVideo".Translate()))
            {
                System.Diagnostics.Process.Start("https://space.bilibili.com/1306433");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("PluginUpdate".Translate()))
            {
                System.Diagnostics.Process.Start("https://github.com/xiaoye97/VRoidXYTool/releases/latest");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 配置界面
        /// </summary>
        public void ConfigGUI()
        {
            GUILayout.BeginVertical("Config".Translate(), GUI.skin.window);
            if (GUILayout.Button("OpenConfigFile".Translate()))
            {
                System.Diagnostics.Process.Start("NotePad.exe", $"{Paths.BepInExRootPath}/config/{PluginID}.cfg");
            }
            GUILayout.BeginHorizontal(GUI.skin.box);
            ShowCameraToolGUI.Value = GUILayout.Toggle(ShowCameraToolGUI.Value, "CameraTool".Translate());
            GUILayout.Space(10);
            ShowGuideToolGUI.Value = GUILayout.Toggle(ShowGuideToolGUI.Value, "GuideTool".Translate());
            GUILayout.Space(10);
            ShowLinkTextureToolGUI.Value = GUILayout.Toggle(ShowLinkTextureToolGUI.Value, "LinkTextureTool".Translate());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUI.skin.box);
            RunInBG.Value = GUILayout.Toggle(RunInBG.Value, "RunInBG".Translate());
            GUILayout.Space(10);
            CameraTool.AntiAliasing.Value = GUILayout.Toggle(CameraTool.AntiAliasing.Value, "AntiAliasing".Translate());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MainViewModel), MethodType.Constructor, new Type[] { typeof(VRoid.UI.BindableResources), typeof(VRoid.Studio.MainModel), typeof(VRoid.UI.CompositionRenderer), typeof(VRoid.UI.GenericCamera.MultipleRenderTextureCamera), typeof(VRoid.UI.Component.AvatarCameraPosition), typeof(VRoid.UI.Component.AvatarCameraPosition), typeof(VRoid.Studio.PhotoBoothScreen.IObsoleteLegacyPhotoBoothViewModel), typeof(UnityTablet.UnityTabletPlugin), typeof(VRoidSDK.SDKConfiguration) })]
        public static void MainVMPatch(MainViewModel __instance)
        {
            XYTool.Inst.MainVM = __instance;
            Debug.Log($"构造了MainViewModel");
        }
    }
}