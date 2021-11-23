using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRoidXYTool
{
    public class TransformControl
    {
        public Transform transform;

        private Dictionary<string, string> floatCache = new Dictionary<string, string>();

        public void OnGUI()
        {
            TransformGUI();
        }

        private void TransformGUI()
        {
            GUILayout.BeginHorizontal();
            transform.position = Vector3GUI(transform.position, "位置", "pos");
            transform.localEulerAngles = Vector3GUI(transform.localEulerAngles, "旋转", "rot");
            transform.localScale = Vector3GUI(transform.localScale, "缩放", "scl");
            GUILayout.EndHorizontal();
        }

        private Vector3 Vector3GUI(Vector3 v, string name, string key)
        {
            GUILayout.BeginVertical(name, GUI.skin.box);
            GUILayout.Space(16);
            v.x = FloatGUI(v.x, "x", key + "_x");
            v.y = FloatGUI(v.y, "y", key + "_y");
            v.z = FloatGUI(v.z, "z", key + "_z");
            GUILayout.EndVertical();
            return v;
        }

        private float FloatGUI(float f, string name, string key)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name, GUILayout.Width(name.Length * 10));
            if (!floatCache.ContainsKey(key))
            {
                floatCache.Add(key, f.ToString());
            }
            floatCache[key] = GUILayout.TextField(floatCache[key]);
            if (float.TryParse(floatCache[key], out float value))
            {
                f = value;
            }
            GUILayout.EndHorizontal();
            return f;
        }
    }
}
