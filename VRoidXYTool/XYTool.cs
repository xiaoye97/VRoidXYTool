using System;
using BepInEx;
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
        public const string PluginVersion = "0.2.2";

        public bool showWindow;
        private Rect winRect = new Rect(50, 50, 500, 600);

        public static XYTool Inst;

        public CurrentFileModel CurrentModelFile;
        public CurrentFileViewModel CurrentViewModelFile;

        public CameraTool CameraTool;
        public GizmoTool GizmoTool;
        public LinkTextureTool LinkTextureTool;

        public ConfigEntry<bool> RunInBG;
        public ConfigEntry<KeyCode> Hotkey;

        public bool RunInBackGround
        {
            get
            {
                return Application.runInBackground;
            }
            set
            {
                if (Application.runInBackground != value)
                {
                    Application.runInBackground = value;
                    RunInBG.Value = value;
                }
            }
        }

        private void Start()
        {
            Inst = this;
            RunInBG = Config.Bind<bool>("Common", "RunInBG", true, "软件是否在后台运行");
            Hotkey = Config.Bind<KeyCode>("Common", "Hotkey", KeyCode.F11, "界面快捷键");
            Application.runInBackground = RunInBG.Value;

            CameraTool = new CameraTool();
            GizmoTool = new GizmoTool();
            LinkTextureTool = new LinkTextureTool();
            Harmony.CreateAndPatchAll(typeof(XYTool));
            Logger.LogInfo("XYTool启动");
        }

        private void Update()
        {
            if (Input.GetKeyDown(Hotkey.Value))
            {
                showWindow = !showWindow;
            }
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
            RunInBackGround = GUILayout.Toggle(RunInBackGround, "软件后台运行");
            GUILayout.Space(20);
            //GUILayout.Label($"插件界面快捷键:{Hotkey.Value}");
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
            CameraTool.OnGUI();
            GizmoTool.OnGUI();
            LinkTextureTool.OnGUI();
            GUI.DragWindow();
        }

        public void InfoGUI()
        {
            GUILayout.BeginVertical("关于", GUI.skin.window);
            GUILayout.Label("作者:xiaoye97");
            GUILayout.BeginHorizontal();
            GUILayout.Label("教程见B站:");
            if (GUILayout.Button("宵夜97", GUILayout.Width(100)))
            {
                System.Diagnostics.Process.Start("https://space.bilibili.com/1306433");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("插件更新见GitHub:");
            if (GUILayout.Button("xiaoye97", GUILayout.Width(100)))
            {
                System.Diagnostics.Process.Start("https://github.com/xiaoye97/VRoidXYTool/releases/latest");
            }
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

        [HarmonyPostfix, HarmonyPatch(typeof(CurrentFileViewModel), MethodType.Constructor, new Type[] { typeof(VRoid.UI.BindableResources), typeof(VRoid.Studio.CurrentFileModel), typeof(VRoid.Studio.View.EditModelTransform), typeof(VRoid.Studio.View.PreviewModelTransform) })]
        public static void ViewModelPatch(CurrentFileViewModel __instance, CurrentFileModel model)
        {
            XYTool.Inst.CurrentViewModelFile = __instance;
            XYTool.Inst.CurrentModelFile = model;
            Debug.Log($"构造了CurrentViewModelFile");
            XYTool.Inst.LinkTextureTool.Clear();
        }
    }
}