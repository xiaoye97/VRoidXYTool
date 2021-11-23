using System;
using UnityEngine;
using VRoid.Studio.Util;
using System.Collections.Generic;

namespace VRoidXYTool
{
    public class GuideTool
    {
        public GameObject GridBox;

        private GameObject boxPrefab;
        private Material guideImageMat;

        private GuidePresetData nowPreset = new GuidePresetData();
        private List<GuideObject> nowObjects = new List<GuideObject>();

        public GuideTool()
        {
            boxPrefab = FileHelper.LoadAsset<GameObject>("guide", "box");
            guideImageMat = FileHelper.LoadAsset<Material>("guide", "GuideImageMat");
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical("参考工具", GUI.skin.window);
            try
            {
                GridBoxGUI();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("加载预设"))
                {

                }
                if (GUILayout.Button("保存预设"))
                {

                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("添加参考图"))
                {
                    AddGuideImage();
                }
                foreach (var obj in nowObjects)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(obj.GuideName);
                    GUILayout.FlexibleSpace();
                    if (obj.IsVaild)
                    {
                        obj.NowEditTransform = GUILayout.Toggle(obj.NowEditTransform, "调整位置");
                        if (GUILayout.Button("删除"))
                        {

                        }
                    }
                    else
                    {
                        GUILayout.Label("加载失败");
                    }
                    GUILayout.EndHorizontal();
                    if (obj.IsVaild)
                    {
                        if (obj.NowEditTransform)
                        {
                            obj.TransformControl.OnGUI();
                        }
                    }
                    GUILayout.EndVertical();
                }
            }
            catch (Exception e)
            {
                GUILayout.Label($"出现异常:{e.Message}\n{e.StackTrace}");
            }
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 添加参考图片
        /// </summary>
        private async void AddGuideImage()
        {
            var path = await FileDialogUtil.OpenFilePanel("选择参考图", null, FileHelper.GeImageFilters(), false);
            if (path == null) return;
            GuideImageData data = new GuideImageData();
            var tex = FileHelper.LoadTexture2D(path[0]);
            if (tex == null) return;
            data.Path = path[0];
            // 设置参考图初始状态
            data.Pos = new V3(0, tex.height / 2000f, -1);
            data.Rot = new V3(0, 180, 0);
            data.Scale = new V3(tex.width / 1000f, tex.height / 1000f, 1);
            nowPreset.Images.Add(data);
            CreateGuideImageObject(data, tex);
        }

        /// <summary>
        /// 创建参考图物体
        /// </summary>
        private void CreateGuideImageObject(GuideImageData data, Texture2D texture2D = null)
        {
            Texture2D tex;
            // 如果有传入的纹理，则使用传入的
            if (texture2D != null)
            {
                tex = texture2D;
            }
            // 如果没有传入的纹理，则从硬盘加载
            else
            {
                tex = FileHelper.LoadTexture2D(data.Path);
            }

            GuideObject guideObject = new GuideObject();
            // 名字
            guideObject.GuideName = $"参考图-{System.IO.Path.GetFileName(data.Path)}";
            guideObject.ObjectType = GuideObjectType.Image;
            nowObjects.Add(guideObject);
            if (tex == null) return;
            // 创建模型
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var r = quad.GetComponent<Renderer>();
            r.material = new Material(guideImageMat);
            r.material.SetTexture("_MainTex", tex);
            // 设置transform
            quad.transform.localScale = data.Scale.ToVector3();
            quad.transform.position = data.Pos.ToVector3();
            quad.transform.localEulerAngles = data.Rot.ToVector3();
            // 设置guideObject
            guideObject.GO = quad;
            guideObject.TransformControl = new TransformControl();
            guideObject.TransformControl.transform = quad.transform;
            guideObject.ImageData = data;
            guideObject.IsVaild = true;
        }

        /// <summary>
        /// 标尺格子的界面
        /// </summary>
        private void GridBoxGUI()
        {
            if (GridBox == null)
            {
                if (GUILayout.Button("实例化标尺格子"))
                {
                    GridBox = GameObject.Instantiate(boxPrefab);
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
                    if (GUILayout.Button("显示标尺格子"))
                    {
                        GridBox.SetActive(true);
                    }
                }
            }
        }
    }
}