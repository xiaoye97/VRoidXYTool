using System;
using BepInEx;
using XYModLib;
using UnityEngine;

namespace VRoidXYTool
{
    public partial class XYTool
    {
        public static Color HeadColor = new Color(84 / 255f, 204 / 255f, 255 / 255f);
        private UIWindow Window;
        private UIWindow MiniWindow;
        // 分页的名字
        private string[] pageNames;
        private Action[] pageGUIActions;
        // 当前分页索引
        private int nowPage;
        // 小窗口的最后一个页面
        private int miniWindowLastPage;

        private Texture2D headTex;

        /// <summary>
        /// 初始化UI窗口
        /// </summary>
        private void InitWindow()
        {
            if (Window == null)
            {
                Window = new UIWindow(string.Format("PluginWindowTitle".Translate(), PluginVersion));
            }
            else
            {
                Window.Name = string.Format("PluginWindowTitle".Translate(), PluginVersion);
            }
            // 窗口尺寸
            Window.WindowRect = new Rect(Screen.width / 2 - 250, Screen.height / 2 - 200, 500, 400);
            Window.OnWinodwGUI = WindowFunc;
            Window.Show = true;
            // 分页信息
            pageNames = new string[]
            {
                "Home".Translate(),
                "CameraTool".Translate(),
                "GuideTool".Translate(),
                "LinkTextureTool".Translate(),
                "PosePersetTool".Translate(),
                "MMD(实验性)",
                "VideoRecord"
            };
            pageGUIActions = new Action[]
            {
                HomePage,
                CameraTool.OnGUI,
                GuideTool.OnGUI,
                LinkTextureTool.OnGUI,
                PosePersetTool.OnGUI,
                MMDTool.OnGUI,
                VideoTool.OnGUI
            };

            // 小窗口
            MiniWindow = new UIWindow("MiniWindow");
            MiniWindow.WindowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2, 300, 10);
            MiniWindow.OnWinodwGUI = MiniWindowFunc;
        }

        private void OnGUI()
        {
            if (Window.Show)
            {
                Window.OnGUI();
            }
            else
            {
                // 右上角的小按钮
                var oriSkin = GUI.skin;
                GUI.skin = ModSkin.Skin;
                if (GUI.Button(new Rect(Screen.width - 26, 4, 22, 22), "夜"))
                {
                    Window.Show = true;
                }
                GUI.skin = oriSkin;
            }
            if (nowPage != miniWindowLastPage)
            {
                miniWindowLastPage = nowPage;
                var r = MiniWindow.WindowRect;
                MiniWindow.WindowRect = new Rect(r.position.x, r.position.y, 300, 10);
            }
            MiniWindow.Name = pageNames[nowPage];
            MiniWindow.OnGUI();
            if (MessageWindow.Inst != null)
            {
                MessageWindow.Inst.Window.OnGUI();
            }
        }

        /// <summary>
        /// 小工具主界面
        /// </summary>
        private void WindowFunc()
        {
            GUILayout.BeginHorizontal();
            // 左侧导航
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(100), GUILayout.ExpandHeight(true));
            int lastPage = nowPage;
            nowPage = GUILayout.SelectionGrid(nowPage, pageNames, 1);
            if (lastPage != nowPage)
            {
                GUIHelper.ClearCache();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            // 右侧顶部信息
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.FlexibleSpace();
            GUILayout.Label(pageNames[nowPage]);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                Window.Show = false;
            }
            GUILayout.EndHorizontal();

            // 显示当前页面
            try
            {
                pageGUIActions[nowPage]();
            }
            catch (Exception ex)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label($"Exception:\n{ex.Message}\n{ex.StackTrace}");
                GUI.contentColor = Color.white;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 小窗口
        /// </summary>
        private void MiniWindowFunc()
        {
            try
            {
                pageGUIActions[nowPage]();
            }
            catch (Exception ex)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label($"Exception:\n{ex.Message}\n{ex.StackTrace}");
                GUI.contentColor = Color.white;
            }
        }

        /// <summary>
        /// 主页界面
        /// </summary>
        private void HomePage()
        {
            InfoGUI();
            ConfigGUI();
            LanguageGUI();
            //TestGUI();
        }

        /// <summary>
        /// 关于界面
        /// </summary>
        private void InfoGUI()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.BeginVertical();
            GUILayout.Label("Author".Translate());
            if (GUILayout.Button("TutorialVideo".Translate()))
            {
                System.Diagnostics.Process.Start("https://space.bilibili.com/1306433");
            }
            if (GUILayout.Button("PluginUpdate".Translate()))
            {
                System.Diagnostics.Process.Start("https://github.com/xiaoye97/VRoidXYTool/releases/latest");
            }
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label(headTex);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 配置界面
        /// </summary>
        private void ConfigGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("OpenConfigFile".Translate()))
            {
                System.Diagnostics.Process.Start("NotePad.exe", $"{Paths.BepInExRootPath}/config/{PluginID}.cfg");
            }
            GUILayout.Label("GUIHotKey".Translate() + ":" + GUIHotkey.Value);
            RunInBG.Value = GUILayout.Toggle(RunInBG.Value, "RunInBG".Translate());
            CameraTool.AntiAliasing.Value = GUILayout.Toggle(CameraTool.AntiAliasing.Value, "AntiAliasing".Translate());
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 选择语言
        /// </summary>
        private void LanguageGUI()
        {
            GUI.contentColor = HeadColor;
            GUILayout.BeginVertical("Language", GUI.skin.window);
            GUI.contentColor = Color.white;
            if (GUILayout.Button("切换到简体中文"))
            {
                SetLanguage(SystemLanguage.ChineseSimplified);
            }
            if (GUILayout.Button("Switch to English"))
            {
                SetLanguage(SystemLanguage.English);
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public void SetLanguage(SystemLanguage lang)
        {
            PluginLanguage.Value = lang;
            I18N.SetLanguage(PluginLanguage.Value);
            InitWindow();
        }

        /// <summary>
        /// 测试代码的GUI
        /// </summary>
        private void TestGUI()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            if (GUILayout.Button("Test"))
            {
                
            }
            GUILayout.EndVertical();
        }
    }
}
