using System;
using UnityEngine;

namespace VRoidXYTool
{
    public class GuideTool
    {
        public GameObject GridBox;

        public void OnGUI()
        {
            GUILayout.BeginVertical("参考工具", GUI.skin.window);
            try
            {
                if (GridBox == null)
                {
                    if (GUILayout.Button("实例化标尺格子"))
                    {
                        var prefab = XYTool.LoadAsset<GameObject>("box", "box");
                        GridBox = GameObject.Instantiate(prefab);
                        GridBox.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                        GridBox.transform.position = new Vector3(0, -0.05f, 0);
                    }
                }
                else
                {
                    if (GridBox.activeSelf)
                    {
                        if (GUILayout.Button("隐藏标尺格子"))
                        {
                            GridBox.SetActive(false);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("显示Gizmo"))
                        {
                            GridBox.SetActive(true);
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