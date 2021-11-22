using System;
using BepInEx;
using VRoid.UI;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio;
using System.Reflection;
using BepInEx.Configuration;

namespace VRoidXYTool
{
    [BepInPlugin(PluginID, PluginName, PluginVersion)]
    public class XYTool : BaseUnityPlugin
    {
        public const string PluginID = "me.xiaoye97.plugin.VRoidStudio.VRoidXYTool";
        public const string PluginName = "VRoidXYTool";
        public const string PluginVersion = "0.2.7";

        public bool showWindow;
        private Rect winRect = new Rect(50, 50, 500, 600);

        public static XYTool Inst;

        public MainViewModel MainVM;

        public CameraTool CameraTool;
        public GuideTool GuideTool;
        public LinkTextureTool LinkTextureTool;

        #region 配置
        public ConfigEntry<bool> RunInBG;
        public ConfigEntry<KeyCode> Hotkey;

        public ConfigEntry<bool> ShowCameraToolGUI;
        public ConfigEntry<bool> ShowGizmoToolGUI;
        public ConfigEntry<bool> ShowLinkTextureToolGUI;
        #endregion

        private void Awake()
        {
            Inst = this;
            RunInBG = Config.Bind<bool>("Common", "RunInBG", true, "软件是否在后台运行");
            Hotkey = Config.Bind<KeyCode>("Common", "Hotkey", KeyCode.F11, "界面快捷键");
            ShowCameraToolGUI = Config.Bind<bool>("Common", "ShowCameraToolGUI", true, "是否显示相机工具GUI");
            ShowGizmoToolGUI = Config.Bind<bool>("Common", "ShowGizmoToolGUI", true, "是否显示Gizmo工具GUI");
            ShowLinkTextureToolGUI = Config.Bind<bool>("Common", "ShowLinkTextureToolGUI", true, "是否显示链接纹理工具GUI");
            Harmony.CreateAndPatchAll(typeof(XYTool));
            Logger.LogInfo("XYTool启动");
        }

        private void Start()
        {
            CameraTool = new CameraTool();
            GuideTool = new GuideTool();
            LinkTextureTool = new LinkTextureTool();
        }

        private void Update()
        {
            if (Input.GetKeyDown(Hotkey.Value))
            {
                showWindow = !showWindow;
            }
            if (Application.runInBackground != RunInBG.Value)
            {
                Application.runInBackground = RunInBG.Value;
            }
            CameraTool.Update();
            LinkTextureTool.Update();
        }

        private void OnGUI()
        {
            if (showWindow)
            {
                GUI.backgroundColor = Color.black;
                winRect = GUILayout.Window(666, winRect, WindowFunc, $"宵夜小工具 v{PluginVersion}");
            }
        }

        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            RunInBG.Value = GUILayout.Toggle(RunInBG.Value, "软件后台运行");
            GUILayout.Space(20);
            if (GUILayout.Button("打开插件配置文件"))
            {
                System.Diagnostics.Process.Start("NotePad.exe", $"{Paths.BepInExRootPath}/config/{PluginID}.cfg");
            }
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
            if (ShowGizmoToolGUI.Value)
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
            GUILayout.BeginVertical("关于", GUI.skin.window);

            GUILayout.BeginHorizontal();
            GUILayout.Label("作者:宵夜97");
            GUILayout.Space(10);
            if (GUILayout.Button("教程视频(B站)", GUILayout.Width(100)))
            {
                System.Diagnostics.Process.Start("https://space.bilibili.com/1306433");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("插件更新", GUILayout.Width(100)))
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
            GUILayout.BeginVertical("配置", GUI.skin.window);

            GUILayout.BeginHorizontal();
            ShowCameraToolGUI.Value = GUILayout.Toggle(ShowCameraToolGUI.Value, "相机工具");
            GUILayout.Space(10);
            ShowGizmoToolGUI.Value = GUILayout.Toggle(ShowGizmoToolGUI.Value, "Gizmo工具");
            GUILayout.Space(10);
            ShowLinkTextureToolGUI.Value = GUILayout.Toggle(ShowLinkTextureToolGUI.Value, "链接纹理工具");
            GUILayout.Space(10);
            CameraTool.AntiAliasing.Value = GUILayout.Toggle(CameraTool.AntiAliasing.Value, "抗锯齿");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

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
            return ab.LoadAsset<T>(assetName);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(MainViewModel), MethodType.Constructor, new Type[] { typeof(VRoid.UI.BindableResources), typeof(VRoid.Studio.MainModel), typeof(VRoid.UI.CompositionRenderer), typeof(VRoid.UI.GenericCamera.MultipleRenderTextureCamera), typeof(VRoid.UI.Component.AvatarCameraPosition), typeof(VRoid.UI.Component.AvatarCameraPosition), typeof(VRoid.Studio.PhotoBoothScreen.IObsoleteLegacyPhotoBoothViewModel), typeof(UnityTablet.UnityTabletPlugin), typeof(VRoidSDK.SDKConfiguration) })]
        public static void MainVMPatch(MainViewModel __instance)
        {
            XYTool.Inst.MainVM = __instance;
            Debug.Log($"构造了MainViewModel");
        }
    }
}