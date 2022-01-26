using UnityEngine;
using System;
using HarmonyLib;

namespace XYModLib
{
    public class RayBlocker
    {
        private RectTransform rt;
        private GameObject canvasObj;
        private GameObject rayblockerObj;
        private Func<Rect> getRectFunc;
        private Func<bool> getShowFunc;
        private RayBlockerMono mono;

        public RayBlocker(Func<Rect> getRect, Func<bool> getShow)
        {
            getRectFunc = getRect;
            getShowFunc = getShow;
            canvasObj = new GameObject("RayBlockerCanvas");
            GameObject.DontDestroyOnLoad(canvasObj);
            canvasObj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            Type GraphicRaycasterType = AccessTools.TypeByName("UnityEngine.UI.GraphicRaycaster");
            Type ImageType = AccessTools.TypeByName("UnityEngine.UI.Image");
            canvasObj.AddComponent(GraphicRaycasterType);
            rayblockerObj = new GameObject("RayBlocker");
            rt = rayblockerObj.AddComponent<RectTransform>();
            rt.SetParent(canvasObj.transform);
            rt.pivot = new Vector2(0, 1);
            var rbImage = rayblockerObj.AddComponent(ImageType);
            Traverse.Create(rbImage).Property("color").SetValue(Color.clear);
            Traverse.Create(rbImage).Property("raycastTarget").SetValue(true);

            mono = canvasObj.AddComponent<RayBlockerMono>();
            mono.OnUpdate = Update;
        }

        public void Update()
        {
            if (getRectFunc != null && getShowFunc != null)
            {
                if (getShowFunc())
                {
                    var rect = getRectFunc();
                    rt.sizeDelta = rect.size;
                    rt.position = new Vector2(rect.position.x, Screen.height - rect.position.y);
                    if (!rayblockerObj.activeSelf)
                    {
                        rayblockerObj.SetActive(true);
                    }
                }
                else
                {
                    rt.sizeDelta = Vector2.zero;
                    if (rayblockerObj.activeSelf)
                    {
                        rayblockerObj.SetActive(false);
                    }
                }
            }
        }
    }
}