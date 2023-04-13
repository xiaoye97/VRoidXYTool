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
/*
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
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("PosePersetTool.ResetPose".Translate()))
                {
                    ResetPose();
                }
                if (GUILayout.Button("PosePersetTool.SaveNowPose".Translate()))
                {
                    SaveNowPoseAsync();
                }
                GUILayout.EndHorizontal();
                sv = GUILayout.BeginScrollView(sv, GUILayout.MinHeight(300));
                foreach (var pose in PoseFileNames)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(pose);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Common.Load".Translate()))
                    {
                        LoadPose(pose);
                    }
                    if (GUILayout.Button("Common.Delete".Translate()))
                    {
                        DeletePose(pose);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUI.contentColor = Color.yellow;
                GUILayout.Label("PosePersetTool.MustPoseMode".Translate());
                GUI.contentColor = Color.white;
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
            var path = await SaveFileDialogUtil.SaveFilePanel("GuideTool.SelectSavePath".Translate(), WorkDirPath, "PoseFile.posejson", FileHelper.GetPoseJsonFilters());
            if (path == null) return;
            if (string.IsNullOrEmpty(path)) return;
            // 导出Pose数据
            var data = PosesViewModel._posesModel._poseController.ExportPose();
            PoseData poseData = new PoseData(data);
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
*/