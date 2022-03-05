using System;
using BepInEx;
using System.Linq;
using UnityEngine;
using VRoid.UI.Component;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace VRoidXYTool
{
    /// <summary>
    /// 相机工具
    /// </summary>
    public class CameraTool
    {
        private MultipleRenderTextureCamera _MRTcamera;
        public MultipleRenderTextureCamera MRTcamera
        {
            get
            {
                if (_MRTcamera == null)
                {
                    try
                    {
                        _MRTcamera = XYTool.Inst.MainVM.GlobalBus.Common3D.AvatarCamera.Body;
                    }
                    catch
                    { }
                }
                return _MRTcamera;
            }
        }

        private Camera _MainCamera;
        public Camera MainCamera
        {
            get
            {
                if (_MainCamera == null)
                {
                    try
                    {
                        _MainCamera = MRTcamera._instantiatedCameras.Keys.First().PrimaryCamera;
                    }
                    catch
                    { }
                }
                return _MainCamera;
            }
        }

        /// <summary>
        /// 抗锯齿
        /// </summary>
        public ConfigEntry<bool> AntiAliasing;

        /// <summary>
        /// 抗锯齿级别
        /// </summary>
        public ConfigEntry<int> AntiAliasingLevel;

        /// <summary>
        /// 镜头预设数据
        /// </summary>
        public CameraPosPresetData CameraPosPresetData;

        /// <summary>
        /// 镜头预设数据的保存路径
        /// </summary>
        public string CameraPosPresetPath;

        public CameraTool()
        {
            AntiAliasing = XYTool.Inst.Config.Bind<bool>("CameraTool", "AntiAliasing", true, "XYTool.AntiAliasing".Translate());
            AntiAliasingLevel = XYTool.Inst.Config.Bind<int>("CameraTool", "AntiAliasingLevel", 8, "CameraTool.AntiAliasingLevelDesc".Translate());
            if (AntiAliasingLevel.Value != 2 && AntiAliasingLevel.Value != 4 && AntiAliasingLevel.Value != 8)
            {
                AntiAliasingLevel.Value = 8;
            }
            CameraPosPresetPath = $"{Paths.ConfigPath}/VRoidXYToolCameraPosPreset.json";
            LoadPreset();
        }

        public void Update()
        {
            if (MRTcamera != null && MainCamera != null)
            {
                if (MainCamera.allowMSAA != AntiAliasing.Value)
                {
                    if (AntiAliasing.Value)
                    {
                        QualitySettings.antiAliasing = AntiAliasingLevel.Value;
                    }
                    MainCamera.allowHDR = AntiAliasing.Value;
                    MainCamera.allowMSAA = AntiAliasing.Value;
                }
            }
        }

        public void OnGUI()
        {
            if (MRTcamera == null || MainCamera == null)
            {
                // 没找到相机
                GUILayout.Label("CameraTool.CameraNotFound".Translate());
            }
            else
            {
                // 正交
                if (MainCamera.orthographic)
                {
                    OrthoGUI();
                }
                // 透视
                else
                {
                    NormalGUI();
                }
            }
        }

        public void SetCameraPos(Vector3 pos, Vector3 rot)
        {
            MRTcamera.transform.position = pos;
            MRTcamera.transform.localEulerAngles = rot;
        }

        /// <summary>
        /// 透视模式下GUI
        /// </summary>
        public void NormalGUI()
        {
            if (GUILayout.Button("CameraTool.SetCameraOrthographic".Translate()))
            {
                MainCamera.orthographic = true;
            }
            GUILayout.BeginHorizontal();
            // 身体位置预设
            GUILayout.BeginHorizontal("CameraTool.CameraPosBody".Translate(), GUI.skin.window);
            if (GUILayout.Button("CameraTool.DirFront".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[0], GetAroundAngles()[0]);
            }
            if (GUILayout.Button("CameraTool.DirBack".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[1], GetAroundAngles()[1]);
            }
            if (GUILayout.Button("CameraTool.DirLeft".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[2], GetAroundAngles()[2]);
            }
            if (GUILayout.Button("CameraTool.DirRight".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[3], GetAroundAngles()[3]);
            }
            GUILayout.EndHorizontal();
            // 头部位置预设
            GUILayout.BeginHorizontal("CameraTool.CameraPosHead".Translate(), GUI.skin.window);
            if (GUILayout.Button("CameraTool.DirFront".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[0], GetAroundAngles()[0]);
            }
            if (GUILayout.Button("CameraTool.DirBack".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[1], GetAroundAngles()[1]);
            }
            if (GUILayout.Button("CameraTool.DirLeft".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[2], GetAroundAngles()[2]);
            }
            if (GUILayout.Button("CameraTool.DirRight".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[3], GetAroundAngles()[3]);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            // 自定义预设
            GUILayout.BeginHorizontal("CameraTool.PerspectiveCameraPosPreset".Translate(), GUI.skin.window);
            for (int i = 0; i < CameraPosPresetData.PresetCount; i++)
            {
                GUILayout.BeginVertical();
                var posData = CameraPosPresetData.PerspectiveCameraPosPresets[i];
                if (posData == null)
                {
                    GUI.contentColor = Color.gray;
                    if (GUILayout.Button($"{i}"))
                    {
                    }
                    GUI.contentColor = Color.white;
                    if (GUILayout.Button($"Common.Save".Translate()))
                    {
                        PerspectiveCameraPosPreset preset = new PerspectiveCameraPosPreset();
                        preset.Pos = new V3(MRTcamera.transform.position);
                        preset.Rot = new V3(MRTcamera.transform.localEulerAngles);
                        CameraPosPresetData.PerspectiveCameraPosPresets[i] = preset;
                        SavePreset();
                    }
                }
                else
                {
                    GUI.contentColor = Color.green;
                    if (GUILayout.Button($"{i}"))
                    {
                        SetCameraPos(posData.Pos.ToVector3(), posData.Rot.ToVector3());
                    }
                    GUI.contentColor = Color.white;
                    if (GUILayout.Button($"Common.Clear".Translate()))
                    {
                        CameraPosPresetData.PerspectiveCameraPosPresets[i] = null;
                        SavePreset();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 正交模式下GUI
        /// </summary>
        public void OrthoGUI()
        {
            if (GUILayout.Button("CameraTool.SetCameraPerspective".Translate()))
            {
                MainCamera.orthographic = false;
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("CameraTool.OrthographicSize".Translate(), GUI.skin.window);
            GUILayout.Label($"{MainCamera.orthographicSize}");
            MainCamera.orthographicSize = GUILayout.HorizontalSlider(MainCamera.orthographicSize, 0.1f, 2f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.15")) MainCamera.orthographicSize = 0.15f;
            if (GUILayout.Button("0.2")) MainCamera.orthographicSize = 0.2f;
            if (GUILayout.Button("0.3")) MainCamera.orthographicSize = 0.3f;
            if (GUILayout.Button("0.8")) MainCamera.orthographicSize = 0.8f;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("CameraTool.CameraPosBody".Translate(), GUI.skin.window);
            if (GUILayout.Button("CameraTool.DirFront".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[0], GetAroundAngles()[0]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("CameraTool.DirBack".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[1], GetAroundAngles()[1]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("CameraTool.DirLeft".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[2], GetAroundAngles()[2]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("CameraTool.DirRight".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[3], GetAroundAngles()[3]);
                MainCamera.orthographicSize = 0.8f;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("CameraTool.CameraPosHead".Translate(), GUI.skin.window);
            if (GUILayout.Button("CameraTool.DirFront".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[0], GetAroundAngles()[0]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("CameraTool.DirBack".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[1], GetAroundAngles()[1]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("CameraTool.DirLeft".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[2], GetAroundAngles()[2]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("CameraTool.DirRight".Translate()))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[3], GetAroundAngles()[3]);
                MainCamera.orthographicSize = 0.15f;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            // 自定义预设
            GUILayout.BeginHorizontal("CameraTool.OrthographicCameraPosPreset".Translate(), GUI.skin.window);
            for (int i = 0; i < CameraPosPresetData.PresetCount; i++)
            {
                GUILayout.BeginVertical();
                var posData = CameraPosPresetData.OrthographicCameraPosPresets[i];
                if (posData == null)
                {
                    GUI.contentColor = Color.gray;
                    if (GUILayout.Button($"{i}"))
                    {
                    }
                    GUI.contentColor = Color.white;
                    if (GUILayout.Button($"Common.Save".Translate()))
                    {
                        OrthographicCameraPosPreset preset = new OrthographicCameraPosPreset();
                        preset.Pos = new V3(MRTcamera.transform.position);
                        preset.Rot = new V3(MRTcamera.transform.localEulerAngles);
                        preset.OrthographicSize = MainCamera.orthographicSize;
                        CameraPosPresetData.OrthographicCameraPosPresets[i] = preset;
                        SavePreset();
                    }
                }
                else
                {
                    GUI.contentColor = Color.green;
                    if (GUILayout.Button($"{i}"))
                    {
                        SetCameraPos(posData.Pos.ToVector3(), posData.Rot.ToVector3());
                        MainCamera.orthographicSize = posData.OrthographicSize;
                    }
                    GUI.contentColor = Color.white;
                    if (GUILayout.Button($"Common.Clear".Translate()))
                    {
                        CameraPosPresetData.OrthographicCameraPosPresets[i] = null;
                        SavePreset();
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 根据中心和半径获取点四周的位置
        /// 以Z轴作为前方
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <returns>四周的位置 前后左右</returns>
        public static Vector3[] GetAroundPos(Vector3 center, float radius)
        {
            Vector3[] result = new Vector3[4];
            result[0] = new Vector3(center.x, center.y, center.z + radius);
            result[1] = new Vector3(center.x, center.y, center.z - radius);
            result[2] = new Vector3(center.x - radius, center.y, center.z);
            result[3] = new Vector3(center.x + radius, center.y, center.z);
            return result;
        }

        /// <summary>
        /// 获得四周的角度
        /// 以Z轴为前方
        /// </summary>
        /// <returns></returns>
        public static Vector3[] GetAroundAngles()
        {
            Vector3[] result = new Vector3[4];
            result[0] = new Vector3(0, 180, 0);
            result[1] = Vector3.zero;
            result[2] = new Vector3(0, 90, 0);
            result[3] = new Vector3(0, 270, 0);
            return result;
        }

        /// <summary>
        /// 加载镜头预设
        /// </summary>
        public void LoadPreset()
        {
            CameraPosPresetData = FileHelper.LoadJson<CameraPosPresetData>(CameraPosPresetPath);
            if (CameraPosPresetData == null)
            {
                CameraPosPresetData = new CameraPosPresetData();
                CameraPosPresetData.PerspectiveCameraPosPresets = new PerspectiveCameraPosPreset[CameraPosPresetData.PresetCount];
                CameraPosPresetData.OrthographicCameraPosPresets = new OrthographicCameraPosPreset[CameraPosPresetData.PresetCount];
            }
        }

        /// <summary>
        /// 保存镜头预设
        /// </summary>
        public void SavePreset()
        {
            FileHelper.SaveJson(CameraPosPresetPath, CameraPosPresetData);
        }
    }
}