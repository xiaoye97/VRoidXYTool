using System;
using BepInEx;
using System.Linq;
using UnityEngine;
using VRoid.UI.Component;
using BepInEx.Configuration;
using System.Collections.Generic;
using VRoidStudio.PhotoBooth;
using HarmonyLib;
using System.IO;
using VRoid.Studio.Util;
using System.Collections;

namespace VRoidXYTool
{
    public class PosePersetTool
    {
        public PosesViewModel PosesViewModel;
        public static string WorkDirPath = "Pose";
        public static DirectoryInfo WorkDir;

        public List<string> PoseFileNames = new List<string>();
        private Vector2 sv;

        public PosePersetTool()
        {
            WorkDir = new DirectoryInfo(WorkDirPath);
            if (!WorkDir.Exists)
            {
                WorkDir.Create();
            }
            RefreshPoseFile();
            Harmony.CreateAndPatchAll(typeof(PosePersetTool));
        }

        public void Clear()
        {
            PosesViewModel = null;
        }

        public void OnGUI()
        {
            if (PosesViewModel != null
                && PosesViewModel._posesModel != null
                && PosesViewModel._posesModel._poseController != null
                && PosesViewModel._posesModel._poseController.enabled)
            {
                if (GUILayout.Button("重置"))
                {
                    ResetPose();
                }
                if (GUILayout.Button("保存当前姿势"))
                {
                    SaveNowPoseAsync();
                }
                sv = GUILayout.BeginScrollView(sv);
                foreach (var pose in PoseFileNames)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(pose);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("读取"))
                    {
                        LoadPose(pose);
                    }
                    if (GUILayout.Button("删除"))
                    {
                        DeletePose(pose);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label($"必须开启摄影棚中的姿势模式才能使用此功能");
            }
        }

        /// <summary>
        /// 重置姿势
        /// </summary>
        public void ResetPose()
        {
            PosesViewModel._selectedIndex = 0;
            PosesViewModel._posesModel.PlayPoseAnimation(0);
            PosesViewModel.Reset();
            PosesViewModel.OnPropertyChanged("SelectedIndex");
        }

        /// <summary>
        /// 保存当前的姿势
        /// </summary>
        public async void SaveNowPoseAsync()
        {
            var path = await FileDialogUtil.SaveFilePanel("SelectSavePath".Translate(), WorkDirPath, "PoseFile.posejson", FileHelper.GetPoseJsonFilters());
            if (path == null) return;
            if (string.IsNullOrEmpty(path)) return;
            // 导出Pose数据
            Dictionary<string, RollControlHandleData> rollControlHandleData;
            var data = EditedExportPose(PosesViewModel._posesModel._poseController, out rollControlHandleData);
            PoseData poseData = new PoseData(data);
            // 记录RollControlHandle数据
            poseData.RollControlHandleData = rollControlHandleData;
            FileHelper.SaveJson(path, poseData);
            RefreshPoseFile();
        }

        /// <summary>
        /// 加载姿势
        /// </summary>
        /// <param name="poseFileName"></param>
        public void LoadPose(string poseFileName)
        {
            PoseData poseData = FileHelper.LoadJson<PoseData>($"Pose/{poseFileName}.posejson");
            if (poseData == null)
            {
                return;
            }
            // 转换数据
            var dict = poseData.ToSerializedPose();
            // 删除老控制器并添加新控制器
            var _this = PosesViewModel._posesModel;
            UnityEngine.Object.DestroyImmediate(_this._poseController);
            _this._animator.Update(0f);
            _this._poseController = _this._vrm.AddComponent<PoseController>();
            _this._poseController.Initialize(dict);
            // 重新放RollControlHandle的数据
            foreach (var ser in _this._poseController.poseSerializers)
            {
                // 如果是RollControlHandle，则从姿势数据中取值赋值
                if (ser is RollControlHandle)
                {
                    var handle = ser as RollControlHandle;
                    if (poseData.RollControlHandleData.ContainsKey(ser.Name))
                    {
                        var data = poseData.RollControlHandleData[ser.Name];
                        // 设置向量
                        handle.localCurrentPoint = data.localCurrentPoint.ToVector3();
                        handle.localStartPoint = data.localStartPoint.ToVector3();
                        // 设置碰撞体的transform
                        var t0 = handle.transform.GetChild(0);
                        var t1 = handle.transform.GetChild(1);
                        data.Collider0Transform.Apply(t0);
                        data.Collider1Transform.Apply(t1);
                        t0.localPosition = Vector3.zero;
                        t1.localPosition = Vector3.zero;
                    }
                }
            }
        }

        /// <summary>
        /// 删除姿势
        /// </summary>
        /// <param name="poseFileName"></param>
        public void DeletePose(string poseFileName)
        {
            string path = $"Pose/{poseFileName}.posejson";
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    RefreshPoseFile();
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 刷新姿势文件
        /// </summary>
        public void RefreshPoseFile()
        {
            PoseFileNames.Clear();
            var files = WorkDir.GetFiles("*.posejson");
            foreach (var file in files)
            {
                PoseFileNames.Add(file.Name.Replace(".posejson", ""));
            }
        }

        /// <summary>
        /// 修改后的ExportPose，会记录更多需要的数据
        /// </summary>
        /// <param name="poseController"></param>
        /// <returns></returns>
        public static Dictionary<string, ISerializedPoseGizmoDefinition> EditedExportPose(PoseController poseController, out Dictionary<string, RollControlHandleData> rollControlHandleData)
        {
            rollControlHandleData = new Dictionary<string, RollControlHandleData>();
            ControlPoint[] componentsInChildren = poseController.controlPointRoot.GetComponentsInChildren<ControlPoint>(true);
            Dictionary<string, ISerializedPoseGizmoDefinition> dictionary = new Dictionary<string, ISerializedPoseGizmoDefinition>();
            foreach (ControlPoint controlPoint in componentsInChildren)
            {
                dictionary.Add(controlPoint.Name, controlPoint.Serialize());
            }
            foreach (IPoseSerializer poseSerializer in poseController.poseSerializers)
            {
                dictionary.Add(poseSerializer.Name, poseSerializer.Serialize());
                // 如果是RollControlHandle，则额外记下Transform的数据
                if (poseSerializer is RollControlHandle)
                {
                    var handle = poseSerializer as RollControlHandle;
                    RollControlHandleData handleData = new RollControlHandleData();
                    // 记录向量
                    handleData.localStartPoint = new V3(handle.localStartPoint);
                    handleData.localCurrentPoint = new V3(handle.localCurrentPoint);
                    // 记录两个碰撞体的transform
                    handleData.Collider0Transform = new TransformData(handle.transform.GetChild(0));
                    handleData.Collider1Transform = new TransformData(handle.transform.GetChild(1));
                    rollControlHandleData[handle.Name] = handleData;
                }
            }
            return dictionary;
        }

        /// <summary>
        /// 获取姿势VM
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix, HarmonyPatch(typeof(PosesViewModel), "Enable")]
        public static void PosesViewModelCtorPatch(PosesViewModel __instance)
        {
            XYTool.Inst.PosePersetTool.PosesViewModel = __instance;
            Debug.Log($"获取了PosesViewModel");
        }
    }
}
