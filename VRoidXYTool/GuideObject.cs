using System;
using UnityEngine;
using System.Collections.Generic;

namespace VRoidXYTool
{
    /// <summary>
    /// 参考物体
    /// </summary>
    public class GuideObject
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsVaild;
        
        public bool IsShow = true;

        public string GuideName;

        public GuideObjectType ObjectType;

        public GameObject GO;

        public Transform Transform;

        public Renderer Renderer;

        /// <summary>
        /// 图片数据，仅在图片类型下存在
        /// </summary>
        public GuideImageData ImageData;

        public bool NowEdit;

        /// <summary>
        /// 销毁
        /// </summary>
        public void Remove()
        {
            GameObject.Destroy(GO);
        }

        /// <summary>
        /// 将运行时的数据保存进对应的Data内，准备保存用
        /// </summary>
        public void Save()
        {
            if (ObjectType == GuideObjectType.Image)
            {
                ImageData.Pos = V3.Parse(Transform.position);
                ImageData.Rot = V3.Parse(Transform.localEulerAngles);
            }
        }

        #region GUI
        public bool InputMode;
        private Dictionary<string, string> floatCache = new Dictionary<string, string>();

        public void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            InputMode = GUILayout.Toggle(InputMode, "InputMode".Translate());

            if (InputMode)
            {
                if (ObjectType == GuideObjectType.Image)
                {
                    ImageData.Scale = FloatGUIInput(ImageData.Scale, "Scale".Translate(), "scl");
                    Transform.localScale = new Vector3(ImageData.Scale * ImageData.Width / 1000f, ImageData.Scale * ImageData.Height / 1000f, 0);
                    float lastOpacity = ImageData.Alpha;
                    ImageData.Alpha = FloatGUIInput(ImageData.Alpha, "Alpha".Translate(), "alpha");
                    if (lastOpacity != ImageData.Alpha)
                    {
                        Renderer.material.SetColor("_Color", new Color(1, 1, 1, ImageData.Alpha));
                    }
                }
                else if (ObjectType == GuideObjectType.Model)
                {
                    Transform.localScale = Vector3GUIInput(Transform.localScale, "Scale".Translate(), "scl");
                }
                GUILayout.EndHorizontal();
                TransformGUIInput();
            }
            else
            {
                floatCache.Clear();
                if (ObjectType == GuideObjectType.Image)
                {
                    ImageData.Scale = FloatGUISlider(ImageData.Scale, "Scale".Translate(), 0, 5);
                    Transform.localScale = new Vector3(ImageData.Scale * ImageData.Width / 1000f, ImageData.Scale * ImageData.Height / 1000f, 0);
                    float lastOpacity = ImageData.Alpha;
                    ImageData.Alpha = FloatGUISlider(ImageData.Alpha, "Alpha".Translate(), 0, 1, 100);
                    if (lastOpacity != ImageData.Alpha)
                    {
                        Renderer.material.SetColor("_Color", new Color(1, 1, 1, ImageData.Alpha));
                    }
                }
                else if (ObjectType == GuideObjectType.Model)
                {
                    Transform.localScale = Vector3GUISlider(Transform.localScale, "Scale".Translate(), 0, 5);
                }
                GUILayout.EndHorizontal();
                TransformGUISlider();
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 输入文本形式的Transform GUI
        /// </summary>
        private void TransformGUIInput()
        {
            GUILayout.BeginHorizontal();
            Transform.position = Vector3GUIInput(Transform.position, "Pos".Translate(), "pos");
            Transform.localEulerAngles = Vector3GUIInput(Transform.localEulerAngles, "Rot".Translate(), "rot");
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 输入文本形式的Vector3 GUI
        /// </summary>
        private Vector3 Vector3GUIInput(Vector3 v, string name, string key)
        {
            GUILayout.BeginVertical(name, GUI.skin.box);
            GUILayout.Space(16);
            v.x = FloatGUIInput(v.x, "x", key + "_x");
            v.y = FloatGUIInput(v.y, "y", key + "_y");
            v.z = FloatGUIInput(v.z, "z", key + "_z");
            GUILayout.EndVertical();
            return v;
        }

        /// <summary>
        /// 输入文本形式的float GUI
        /// </summary>
        private float FloatGUIInput(float f, string name, string key)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(name.Length * 14));
            if (!floatCache.ContainsKey(key))
            {
                floatCache.Add(key, f.ToString());
            }
            floatCache[key] = GUILayout.TextField(floatCache[key], GUILayout.Width(150));
            if (float.TryParse(floatCache[key], out float value))
            {
                f = value;
            }
            GUILayout.EndHorizontal();
            return f;
        }

        /// <summary>
        /// 滑动条形式的Transform GUI
        /// </summary>
        private void TransformGUISlider()
        {
            GUILayout.BeginHorizontal();
            Transform.position = Vector3GUISlider(Transform.position, "Pos".Translate(), -3, 3);
            Transform.localEulerAngles = Vector3GUISlider(Transform.localEulerAngles, "Rot".Translate(), 0, 360);
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 滑动条形式的Vector3 GUI
        /// </summary>
        private Vector3 Vector3GUISlider(Vector3 v, string name, float min, float max)
        {
            GUILayout.BeginVertical(name, GUI.skin.box);
            GUILayout.Space(16);
            v.x = FloatGUISlider(v.x, "x", min, max);
            v.y = FloatGUISlider(v.y, "y", min, max);
            v.z = FloatGUISlider(v.z, "z", min, max);
            GUILayout.EndVertical();
            return v;
        }

        /// <summary>
        /// 滑动条形式的float GUI
        /// </summary>
        private float FloatGUISlider(float f, string name, float min, float max, int nameWidth = 70)
        {
            GUILayout.BeginHorizontal();
            string label = $"{name}:{f:f3}";
            GUILayout.Label(label, GUILayout.Width(nameWidth));
            f = GUILayout.HorizontalSlider(f, min, max, GUILayout.Width(100));
            f = (int)(f * 1000) / 1000f;
            GUILayout.EndHorizontal();
            return f;
        }
        #endregion
    }

    public enum GuideObjectType
    {
        None,
        Image,
        Model
    }
}
