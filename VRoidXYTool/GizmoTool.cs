using System;
using UnityEngine;

namespace VRoidXYTool
{
    public class GizmoTool
    {
        public GameObject PosGizmo;

        public void OnGUI()
        {
            GUILayout.BeginVertical("Gizmo", GUI.skin.window);
            try
            {
                if (PosGizmo == null)
                {
                    if (GUILayout.Button("实例化Gizmo"))
                    {
                        var prefab = XYTool.LoadAsset<GameObject>("box", "box");
                        PosGizmo = GameObject.Instantiate(prefab);
                        PosGizmo.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                        PosGizmo.transform.position = new Vector3(0, -0.05f, 0);
                    }
                }
                else
                {
                    if (PosGizmo.activeSelf)
                    {
                        if (GUILayout.Button("隐藏Gizmo"))
                        {
                            PosGizmo.SetActive(false);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("显示Gizmo"))
                        {
                            PosGizmo.SetActive(true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                GUILayout.Label($"出现异常:{e.Message}\n{e.StackTrace}");
            }
            GUILayout.EndVertical();
        }
    }
}