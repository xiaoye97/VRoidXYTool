using System;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;

namespace VRoidXYTool
{
    public static class InterfaceMaker
    {
        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }

        public static GUISkin CustomSkin
        {
            get
            {
                if (InterfaceMaker._customSkin == null)
                {
                    try
                    {
                        InterfaceMaker._customSkin = InterfaceMaker.CreateSkin();
                    }
                    catch (System.Exception ex)
                    {
                        InterfaceMaker._customSkin = GUI.skin;
                    }
                }
                return InterfaceMaker._customSkin;
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
            byte[] embeddedResource = ResourceUtils.GetEmbeddedResource("guisharp-box.png", null);
            InterfaceMaker._boxBackground = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._boxBackground);
            guiskin.box.onNormal.background = null;
            guiskin.box.normal.background = InterfaceMaker._boxBackground;

            guiskin.box.normal.textColor = Color.white;
            // 窗口图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("guisharp-window.png", null);
            InterfaceMaker._winBackground = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._winBackground);
            guiskin.window.onNormal.background = null;
            guiskin.window.normal.background = InterfaceMaker._winBackground;

            guiskin.window.padding = new RectOffset(6, 6, 22, 6);
            guiskin.window.border = new RectOffset(10, 10, 20, 10);
            guiskin.window.normal.textColor = Color.white;
            guiskin.button.padding = new RectOffset(4, 4, 3, 3);
            guiskin.button.normal.textColor = Color.white;
            guiskin.textField.normal.textColor = Color.white;
            guiskin.label.normal.textColor = Color.white;

            // 按钮图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("button_normal.png", null);
            InterfaceMaker._buttonNormal = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._buttonNormal);
            guiskin.button.normal.background = InterfaceMaker._buttonNormal;

            // 按钮图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("button_hover.png", null);
            InterfaceMaker._buttonHover = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._buttonHover);
            guiskin.button.hover.background = InterfaceMaker._buttonHover;

            // 按钮图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("button_press.png", null);
            InterfaceMaker._buttonPress = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._buttonPress);
            guiskin.button.active.background = InterfaceMaker._buttonPress;

            guiskin.button.onNormal.background = InterfaceMaker._buttonHover;
            guiskin.button.onHover.background = InterfaceMaker._buttonHover;
            guiskin.button.onActive.background = InterfaceMaker._buttonPress;

            // 滚动条底板图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("vertical scrollbar thumb.png", null);
            InterfaceMaker._verticalScrollbarThumb = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._verticalScrollbarThumb);
            guiskin.verticalScrollbarThumb.normal.background = InterfaceMaker._verticalScrollbarThumb;

            // 滚动条图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("vertical scrollbar.png", null);
            InterfaceMaker._verticalScrollbar = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._verticalScrollbar);
            guiskin.verticalScrollbar.normal.background = InterfaceMaker._verticalScrollbar;

            // Toggle图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("toggle.png", null);
            InterfaceMaker._toggleNormal = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._toggleNormal);

            // Toggle图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("toggle on.png", null);
            InterfaceMaker._toggleOnNormal = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._toggleOnNormal);

            guiskin.toggle.normal.background = InterfaceMaker._toggleNormal;
            guiskin.toggle.onNormal.background = InterfaceMaker._toggleOnNormal;
            guiskin.toggle.hover.background = InterfaceMaker._toggleOnNormal;
            guiskin.toggle.onHover.background = InterfaceMaker._toggleOnNormal;
            guiskin.toggle.active.background = InterfaceMaker._toggleNormal;
            guiskin.toggle.onActive.background = InterfaceMaker._toggleOnNormal;

            // 输入框图片
            embeddedResource = ResourceUtils.GetEmbeddedResource("textfield.png", null);
            InterfaceMaker._textfieldNormal = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._textfieldNormal);

            embeddedResource = ResourceUtils.GetEmbeddedResource("textfield on.png", null);
            InterfaceMaker._textfieldOnNormal = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._textfieldOnNormal);

            embeddedResource = ResourceUtils.GetEmbeddedResource("textfield hover.png", null);
            InterfaceMaker._textfieldHover = LoadTexture(embeddedResource);
            Object.DontDestroyOnLoad(InterfaceMaker._textfieldHover);

            guiskin.textField.normal.background = InterfaceMaker._textfieldNormal;
            guiskin.textField.onNormal.background = InterfaceMaker._textfieldOnNormal;
            guiskin.textField.hover.background = InterfaceMaker._textfieldHover;

            return guiskin;
        }
        private static Texture2D _textfieldHover;
        private static Texture2D _textfieldOnNormal;
        private static Texture2D _textfieldNormal;

        private static Texture2D _toggleOnNormal;
        private static Texture2D _toggleNormal;

        private static Texture2D _verticalScrollbarThumb;
        private static Texture2D _verticalScrollbar;

        private static Texture2D _buttonNormal;
        private static Texture2D _buttonHover;
        private static Texture2D _buttonPress;

        private static Texture2D _boxBackground;

        private static Texture2D _winBackground;

        private static GUISkin _customSkin;

        public static Texture2D LoadTexture(byte[] texData)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            MethodInfo method = typeof(Texture2D).GetMethod("LoadImage", new Type[]
            {
        typeof(byte[])
            });
            if (method != null)
            {
                method.Invoke(texture2D, new object[]
                {
            texData
                });
            }
            else
            {
                Type type = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule");
                if (type == null)
                {
                    throw new ArgumentNullException("converter");
                }
                MethodInfo method2 = type.GetMethod("LoadImage", new Type[]
                {
            typeof(Texture2D),
            typeof(byte[])
                });
                if (method2 == null)
                {
                    throw new ArgumentNullException("converterMethod");
                }
                method2.Invoke(null, new object[]
                {
            texture2D,
            texData
                });
            }
            return texture2D;
        }
    }
}
