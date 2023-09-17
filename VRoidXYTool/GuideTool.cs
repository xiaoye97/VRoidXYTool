using System;
using UnityEngine;
using VRoid.Studio.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VRoidXYTool
{
    /// <summary>
    /// 参考工具
    /// </summary>
    public class GuideTool
    {
        public GameObject GridBox;

        /// <summary>
        /// 标尺盒子预制体
        /// </summary>
        private GameObject boxPrefab;

        /// <summary>
        /// 参考图预制体
        /// </summary>
        private GameObject guideImagePrefab;

        /// <summary>
        /// 参考图材质
        /// </summary>
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
            if (XYTool.Inst.IsModelNull)
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label("Common.NowNotLoadModel".Translate());
                GUI.contentColor = Color.white;
            }
            else
            {
                GridBoxGUI();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("GuideTool.LoadPreset".Translate()))
                {
                    LoadPreset();
                }
                if (GUILayout.Button("GuideTool.SavePreset".Translate()))
                {
                    SavePreset();
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("GuideTool.AddGuideImage".Translate()))
                {
                    AddGuideImage();
                }
                foreach (var obj in nowObjects)
                {
                    GUILayout.BeginVertical(GUI.skin.box);
                    GUILayout.BeginHorizontal();
                    if (obj.IsVaild)
                    {
                        obj.IsShow = GUILayout.Toggle(obj.IsShow, obj.GuideName);
                        if (obj.IsShow != obj.GO.activeSelf)
                        {
                            obj.GO.SetActive(obj.IsShow);
                        }
                    }
                    else
                    {
                        GUILayout.Label(obj.GuideName);
                    }
                    GUILayout.FlexibleSpace();
                    if (obj.IsVaild)
                    {
                        obj.NowEdit = GUILayout.Toggle(obj.NowEdit, "GuideTool.EditGuideParameters".Translate());
                        if (GUILayout.Button("GuideTool.DeleteGuideObject".Translate()))
                        {
                            needRemoveObjects.Add(obj);
                        }
                    }
                    else
                    {
                        GUILayout.Label("GuideTool.LoadFail".Translate());
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
        }

        /// <summary>
        /// 加载预设
        /// </summary>
        public async void LoadPreset()
        {
            var path = await OpenFileDialogUtil.OpenFilePanel("GuideTool.SelectPresetFile".Translate(), null, FileHelper.GetJsonFilters(), false);
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
            var path = await SaveFileDialogUtil.SaveFileSingleAsync("GuideTool.SelectSavePath".Translate(), null, "XYToolPreset.json", FileHelper.GetJsonFilter(), (tmpPath) =>
            {
                if (string.IsNullOrEmpty(tmpPath))
                {
                    return Task.FromResult<bool>(false);
                }
                foreach (var obj in nowObjects)
                {
                    obj.Save();
                }
                FileHelper.SaveJson(tmpPath, nowPreset);
                return Task.FromResult<bool>(true);
            });
        }

        /// <summary>
        /// 添加参考图片
        /// </summary>
        private async void AddGuideImage()
        {
            var path = await OpenFileDialogUtil.OpenFilePanel("GuideTool.SelectImage".Translate(), null, FileHelper.GetImageFilters(), false);
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
            data.Alpha = 1f;
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
            guideObject.GuideName = $"{System.IO.Path.GetFileName(data.Path)}";
            guideObject.ObjectType = GuideObjectType.Image;
            nowObjects.Add(guideObject);
            if (tex == null) return;
            // 创建模型
            var image = GameObject.Instantiate(guideImagePrefab);
            guideObject.Renderer = image.GetComponent<Renderer>();
            guideObject.Renderer.material = new Material(guideImageMat);
            guideObject.Renderer.material.SetTexture("_MainTex", tex);
            guideObject.Renderer.material.SetColor("_Color", new Color(1, 1, 1, data.Alpha));
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
                if (GUILayout.Button("GuideTool.CreateGuideGrid".Translate()))
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
                    if (GUILayout.Button("GuideTool.HideGuideGrid".Translate()))
                    {
                        GridBox.SetActive(false);
                    }
                }
                else
                {
                    if (GUILayout.Button("GuideTool.ShowGuideGrid".Translate()))
                    {
                        GridBox.SetActive(true);
                    }
                }
            }
        }
    }
}