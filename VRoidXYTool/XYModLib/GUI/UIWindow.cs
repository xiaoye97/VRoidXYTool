using System;
using UnityEngine;

namespace XYModLib
{
    public class UIWindow
    {
        public int WindowID;
        public Rect WindowRect = new Rect(Screen.width / 2 - 400, Screen.height / 2 - 300, 800, 600);
        public RayBlocker RayBlocker;
        public string Name;

        private bool show;
        public bool Show
        {
            get { return show; }
            set
            {
                if (show != value)
                {
                    show = value;
                    if (show)
                    {
                        if (OnWindowOpen != null) OnWindowOpen();
                    }
                    else
                    {
                        if (OnWindowClose != null) OnWindowClose();
                    }
                }
            }
        }

        public bool CanDrag = true;

        public bool UseModSkin = true;

        public Action OnWindowOpen;
        public Action OnWindowClose;
        public Action OnWinodwGUI;

        public UIWindow(string name)
        {
            Name = name;
            WindowID = UnityEngine.Random.Range(1000000, int.MaxValue);
            RayBlocker = new RayBlocker(() => WindowRect, () => Show);
        }

        public UIWindow(string name, Rect rect)
        {
            Name = name;
            WindowRect = rect;
            WindowID = UnityEngine.Random.Range(1000000, int.MaxValue);
            RayBlocker = new RayBlocker(() => WindowRect, () => Show);
        }
        
        public void OnGUI()
        {
            if (Show)
            {
                var oriSkin = GUI.skin;
                if (UseModSkin)
                    GUI.skin = ModSkin.Skin;
                WindowRect = GUILayout.Window(WindowID, WindowRect, WindowFunc, Name);
                // 还原
                GUI.skin = oriSkin;
            }
        }

        public void WindowFunc(int id)
        {
            if (OnWinodwGUI != null)
            {
                OnWinodwGUI();
            }
            else
            {
                GUILayout.Label(" ");
            }
            if (CanDrag)
            {
                GUI.DragWindow();
            }
        }
    }
}
