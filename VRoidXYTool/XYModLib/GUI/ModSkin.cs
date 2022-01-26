using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace XYModLib
{
    public static class ModSkin
    {
        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }

        public static GUISkin Skin
        {
            get
            {
                if (ModSkin._customSkin == null)
                {
                    try
                    {
                        ModSkin._customSkin = ModSkin.CreateSkin();
                    }
                    catch
                    {
                        ModSkin._customSkin = GUI.skin;
                    }
                }
                return ModSkin._customSkin;
            }
        }

        private static GUISkin CreateSkin()
        {
            // 动态创建UI皮肤
            GUISkin guiskin = typeof(Object).GetMethod("Instantiate", BindingFlags.Static | BindingFlags.Public, null, new Type[]
            {
                typeof(Object)
            }, null).Invoke(null, new object[]
            {
                GUI.skin
            }) as GUISkin;
            Object.DontDestroyOnLoad(guiskin);

            // box图片
            guiskin.box.onNormal.background = null;
            guiskin.box.normal.background = ResourceUtils.GetTex("guisharp-box.png");

            guiskin.box.normal.textColor = Color.white;
            // 窗口图片
            guiskin.window.onNormal.background = null;
            guiskin.window.normal.background = ResourceUtils.GetTex("guisharp-window.png");

            guiskin.window.padding = new RectOffset(6, 6, 22, 6);
            guiskin.window.border = new RectOffset(10, 10, 20, 10);
            guiskin.window.normal.textColor = Color.white;
            guiskin.button.padding = new RectOffset(4, 4, 3, 3);
            guiskin.button.normal.textColor = Color.white;
            guiskin.textField.normal.textColor = Color.white;
            guiskin.label.normal.textColor = Color.white;

            // 按钮图片
            guiskin.button.normal.background = ResourceUtils.GetTex("button_normal.png");

            // 按钮图片
            guiskin.button.hover.background = ResourceUtils.GetTex("button_hover.png");

            // 按钮图片
            guiskin.button.active.background = ResourceUtils.GetTex("button_press.png");

            guiskin.button.onNormal.background = ResourceUtils.GetTex("button_hover.png");
            guiskin.button.onHover.background = ResourceUtils.GetTex("button_hover.png");
            guiskin.button.onActive.background = ResourceUtils.GetTex("button_press.png");

            // 滚动条底板图片
            guiskin.verticalScrollbarThumb.normal.background = ResourceUtils.GetTex("vertical scrollbar thumb.png");

            // 滚动条图片
            guiskin.verticalScrollbar.normal.background = ResourceUtils.GetTex("vertical scrollbar.png");

            // Toggle图片
            guiskin.toggle.normal.background = ResourceUtils.GetTex("toggle.png");
            guiskin.toggle.onNormal.background = ResourceUtils.GetTex("toggle on.png");
            guiskin.toggle.hover.background = ResourceUtils.GetTex("toggle on.png");
            guiskin.toggle.onHover.background = ResourceUtils.GetTex("toggle on.png");
            guiskin.toggle.active.background = ResourceUtils.GetTex("toggle.png");
            guiskin.toggle.onActive.background = ResourceUtils.GetTex("toggle on.png");

            // 输入框图片
            guiskin.textField.normal.background = ResourceUtils.GetTex("textfield.png");
            guiskin.textField.onNormal.background = ResourceUtils.GetTex("textfield on.png");
            guiskin.textField.hover.background = ResourceUtils.GetTex("textfield hover.png");

            return guiskin;
        }

        private static GUISkin _customSkin;
    }
}
