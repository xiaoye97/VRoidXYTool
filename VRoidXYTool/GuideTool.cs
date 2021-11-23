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
        private GameObject guideImagePrefab;
        private Material guideImageMat;

        private GuidePresetData nowPreset = new GuidePresetData();
        private List<GuideObject> nowObjects = new List<GuideObject>();
        private List<GuideObject> needRemoveObjects = new List<GuideObject>();

        public GuideTool()
        {
            boxPrefab = FileHelper.LoadAsset<GameObject>("guide", "box");
            guideImageMat = FileHelper.LoadAsset<Material>("guide", "GuideImageMat");
            guideImagePrefab = FileHelper.LoadAsset<GameObject>("guide", "GuideImagePrefab");
        }

        public void OnGUI()
        {
            // 删除待删除的物体
            if (needRemoveObjects.Count > 0)
            {
                foreach (var obj in needRemoveObjects)
                {
                    nowObjects.Remove(obj);
                    obj.Remove();
                }
                needRemoveObjects.Clear();
            }
            GUILayout.BeginVertical("参考工具", GUI.skin.window);
            try
            {
                GridBoxGUI();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("加载预设"))
                {
                    LoadPreset();
                }
                if (GUILayout.Button("保存预设"))
                {
                    SavePreset();
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
                        obj.NowEdit = GUILayout.Toggle(obj.NowEdit, "调整位置");
                        if (GUILayout.Button("删除"))
                        {
                            needRemoveObjects.Add(obj);
                        }
                    }
                    else
                    {
                        GUILayout.Label("加载失败");
                    }
                    GUILayout.EndHorizontal();
                    if (obj.IsVaild)
                    {
                        if (obj.NowEdit)
                        {
                            obj.OnGUI();
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
        /// 加载预设
        /// </summary>
        public async void LoadPreset()
        {
            var path = await FileDialogUtil.OpenFilePanel("选择预设文件", null, FileHelper.GetJsonFilters(), false);
            if (path == null) return;
            GuidePresetData data = FileHelper.LoadJson<GuidePresetData>(path[0]);
            if (data == null) return;

            // 清理现有的对象
            foreach (var obj in nowObjects)
            {
                obj.Remove();
            }
            nowObjects.Clear();
            needRemoveObjects.Clear();

            // 生成配置中的对象
            nowPreset = data;

            // 生成参考图
            foreach (var image in nowPreset.Images)
            {
                CreateGuideImageObject(image);
            }
        }

        /// <summary>
        /// 保存预设
        /// </summary>
        public async void SavePreset()
        {
            var path = await FileDialogUtil.SaveFilePanel("选择保存位置", null, "XYToolPreset.json", FileHelper.GetJsonFilters());
            if (path == null) return;
            if (string.IsNullOrEmpty(path)) return;
            foreach (var obj in nowObjects)
            {
                obj.Save();
            }
            FileHelper.SaveJson(path, nowPreset);
        }

        /// <summary>
        /// 添加参考图片
        /// </summary>
        private async void AddGuideImage()
        {
            var path = await FileDialogUtil.OpenFilePanel("选择参考图", null, FileHelper.GetImageFilters(), false);
            if (path == null) return;
            GuideImageData data = new GuideImageData();
            var tex = FileHelper.LoadTexture2D(path[0]);
            if (tex == null) return;
            data.Path = path[0];
            // 设置参考图初始状态
            data.Pos = new V3(0, tex.height / 2000f, -1);
            data.Rot = new V3(0, 0, 0);
            data.Width = tex.width;
            data.Height = tex.height;
            data.Scale = 1f;
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
            var image = GameObject.Instantiate(guideImagePrefab);
            var r = image.GetComponent<Renderer>();
            r.material = new Material(guideImageMat);
            r.material.SetTexture("_MainTex", tex);
            // 设置transform
            image.transform.localScale = new Vector3(data.Width / 1000f * data.Scale, data.Height / 1000f * data.Scale, 0);
            image.transform.position = data.Pos.ToVector3();
            image.transform.localEulerAngles = data.Rot.ToVector3();
            // 设置guideObject
            guideObject.GO = image;
            guideObject.Transform = image.transform;
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