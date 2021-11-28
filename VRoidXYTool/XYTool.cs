using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio;
using BepInEx.Configuration;
using UnityEngine.EventSystems;
using VRoid.UI.Component;

namespace VRoidXYTool
{
    [BepInPlugin(PluginID, PluginName, PluginVersion)]
    public class XYTool : BaseUnityPlugin
    {
        public const string PluginID = "me.xiaoye97.plugin.VRoidStudio.VRoidXYTool";
        public const string PluginName = "VRoidXYTool";
        public const string PluginVersion = "0.3.3";

        public bool showWindow;
        private Rect winRect = new Rect(50, 50, 500, 600);

        public static XYTool Inst;

        public MainViewModel MainVM;

        public CameraTool CameraTool;
        public GuideTool GuideTool;
        public LinkTextureTool LinkTextureTool;

        #region 配置
        public ConfigEntry<SystemLanguage> PluginLanguage;
        public ConfigEntry<bool> RunInBG;
        public ConfigEntry<bool> OnGUICantClick;
        public ConfigEntry<KeyCode> Hotkey;

        public ConfigEntry<bool> ShowCameraToolGUI;
        public ConfigEntry<bool> ShowGuideToolGUI;
        public ConfigEntry<bool> ShowLinkTextureToolGUI;
        #endregion

        private EventSystem ES2D; // 2D界面事件系统
        private EventSystem3D ES3D; //  3D界面事件系统

        private void Awake()
        {
            Inst = this;
            I18N.Init();
            PluginLanguage = Config.Bind<SystemLanguage>("Common", "Language", Application.systemLanguage, "Plugin language");
            I18N.SetLanguage(PluginLanguage.Value);

            RunInBG = Config.Bind<bool>("Common", "RunInBG", true, "RunInBGDesc".Translate());
            OnGUICantClick = Config.Bind<bool>("Common", "OnGUICantClick", true, "OnGUICantClick".Translate());
            Hotkey = Config.Bind<KeyCode>("Common", "Hotkey", KeyCode.F11, "GUIHotkey".Translate());
            ShowCameraToolGUI = Config.Bind<bool>("Common", "ShowCameraToolGUI", true, "ShowCameraToolGUI".Translate());
            ShowGuideToolGUI = Config.Bind<bool>("Common", "ShowGizmoToolGUI", true, "ShowGuideToolGUI".Translate());
            ShowLinkTextureToolGUI = Config.Bind<bool>("Common", "ShowLinkTextureToolGUI", true, "ShowLinkTextureToolGUI".Translate());
            Harmony.CreateAndPatchAll(typeof(XYTool));
            Logger.LogInfo("XYTool启动");
            Logger.LogInfo($"当前VRoidStudio版本为{Application.version}");
        }

        private void Start()
        {
            CameraTool = new CameraTool();
            GuideTool = new GuideTool();
            LinkTextureTool = new LinkTextureTool();
        }

        private void Update()
        {
            // 控制界面显示
            if (Input.GetKeyDown(Hotkey.Value))
            {
                showWindow = !showWindow;
            }
            // 控制配置中的值同步
            if (Application.runInBackground != RunInBG.Value)
            {
                Application.runInBackground = RunInBG.Value;
            }
            CheckEventSystem();
            // 工具的Update
            CameraTool.Update();
            LinkTextureTool.Update();
        }

        /// <summary>
        /// 检查事件系统
        /// </summary>
        private void CheckEventSystem()
        {
            if (ES2D == null)
            {
                ES2D = GameObject.FindObjectOfType<EventSystem>();
            }
            if (ES3D == null)
            {
                ES3D = GameObject.FindObjectOfType<EventSystem3D>();
            }
            // 是否启用事件系统
            bool esActive = true;
            if (OnGUICantClick.Value && showWindow)
            {
                // 如果配置打开并且当前在显示小工具界面，则关闭事件系统
                esActive = false;
            }
            if (ES2D.enabled != esActive)
            {
                ES2D.enabled = esActive;
            }
            if (ES3D.enabled != esActive)
            {
                ES3D.enabled = esActive;
            }
        }

        private void OnGUI()
        {
            if (showWindow)
            {
                GUI.backgroundColor = Color.black;
                winRect = GUILayout.Window(666, winRect, WindowFunc, string.Format("PluginWindowTitle".Translate(), PluginVersion));
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
                showWindow = false;
            }
            GUILayout.EndHorizontal();
            InfoGUI();
            ConfigGUI();
            if (ShowCameraToolGUI.Value)
            {
                CameraTool.OnGUI();
            }
            if (ShowGuideToolGUI.Value)
            {
                GuideTool.OnGUI();
            }
            if (ShowLinkTextureToolGUI.Value)
            {
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
            OnGUICantClick.Value = GUILayout.Toggle(OnGUICantClick.Value, "OnGUICantClick".Translate());
            GUILayout.Space(10);
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