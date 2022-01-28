using System;
using XYModLib;
using UnityEngine;

namespace VRoidXYTool
{
    public class MessageWindow
    {
        public static MessageWindow Inst;
        public UIWindow Window;
        public string Title;
        public string Message;

        public MessageWindow()
        {
            Inst = this;
            Window = new UIWindow("Message", new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200));
            Window.OnWindowOpen = OnWindowOpen;
            Window.OnWinodwGUI = OnGUI;
        }

        public void OnWindowOpen()
        {
            Window.Name = Title;
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(Message);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK"))
            {
                Window.Show = false;
            }
            GUILayout.EndVertical();
        }

        public static void Show(string title, string msg)
        {
            Inst = new MessageWindow();
            Inst.Title = title;
            Inst.Message = msg;
            Inst.Window.Show = true;
        }
    }
}
