/*
 参考https://github.com/ginko1/VRoidXYTool/blob/main/VRoidXYTool/RayBlocker.cs
 */

using UnityEngine;
using UnityEngine.UI;

namespace VRoidXYTool
{
    public class RayBlocker
    {
        private RectTransform rt;
        private GameObject canvasObj;

        public RayBlocker()
        {
            canvasObj = new GameObject("VRoidXYToolRayBlockerCanvas");
            canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<GraphicRaycaster>();
            var rayblocker = new GameObject("RayBlocker");
            rt = rayblocker.AddComponent<RectTransform>();
            rt.SetParent(canvasObj.transform);
            rt.pivot = new Vector2(0, 1);
            Image rbImage = rayblocker.AddComponent<Image>();
            rbImage.color = Color.clear;
            rbImage.raycastTarget = true;
            CloseBlocker();
        }

        public void Update()
        {
            rt.sizeDelta = XYTool.Inst.ToolWindowRect.size;
            rt.position = new Vector2(XYTool.Inst.ToolWindowRect.position.x, Screen.height - XYTool.Inst.ToolWindowRect.position.y);
        }

        public void OpenBlocker()
        {
            canvasObj.SetActive(true);
        }

        public void CloseBlocker()
        {
            canvasObj.SetActive(false);
        }
    }
}