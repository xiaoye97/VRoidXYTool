using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using VRoid.Studio;
using System.Reflection;
using BepInEx.Configuration;

namespace VRoidXYTool
{
    [BepInPlugin("me.xiaoye97.plugin.VRoidStudio.VRoidXYTool", "VRoidXYTool", PluginVersion)]
    public class XYTool : BaseUnityPlugin
    {
        public const string PluginVersion = "0.1";
        public bool showWindow;
        private Rect winRect = new Rect(50, 50, 500, 600);

        public static XYTool Inst;

        public CurrentFileModel CurrentModelFile;

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
                winRect = GUILayout.Window(666, winRect, WindowFunc, $"宵夜小工具 v{PluginVersion}");
            }
        }

        public void WindowFunc(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X"))
            {
                showWindow = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical("常规", GUI.skin.window);
            GUILayout.Label("作者:xiaoye97");
            GUILayout.Label("教程见B站:宵夜97");
            GUILayout.Label("插件更新见GitHub:xiaoye97");
            RunInBackGround = GUILayout.Toggle(RunInBackGround, "软件后台运行");
            GUILayout.EndVertical();
            CameraTool.OnGUI();
            GizmoTool.OnGUI();
            LinkTextureTool.OnGUI();
            GUI.DragWindow();
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

        [HarmonyPostfix, HarmonyPatch(typeof(CurrentFileModel), MethodType.Constructor, new Type[] { typeof(VRoid.Studio.Engine.Model), typeof(string) })]
        public static void ModelPatch(CurrentFileModel __instance, VRoid.Studio.Engine.Model engine,  string path)
        {
            XYTool.Inst.CurrentModelFile = __instance;
            Debug.Log($"构造了CurrentFileModel, path:{path}");
        }
    }
}