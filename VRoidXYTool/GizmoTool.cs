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
                    if (GUILayout.Button("实例化坐标系Gizmo"))
                    {
                        var prefab = XYTool.LoadAsset<GameObject>("posgizmo", "posgizmo");
                        PosGizmo = GameObject.Instantiate(prefab);
                    }
                }
                else
                {
                    if (PosGizmo.activeSelf)
                    {
                        if (GUILayout.Button("隐藏坐标系Gizmo"))
                        {
                            PosGizmo.SetActive(false);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("显示坐标系Gizmo"))
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