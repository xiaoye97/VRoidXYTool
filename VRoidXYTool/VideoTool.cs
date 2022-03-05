using System;
using BepInEx;
using XYModLib;
using System.IO;
using UnityEngine;
using System.Reflection;
using BepInEx.Configuration;

namespace VRoidXYTool
{
    /// <summary>
    /// 视频录制工具
    /// </summary>
    public class VideoTool
    {
        private VedioToolState state;

        private string exLog = "";

        private Assembly NatSuite;

        private object recorder;
        private object clock;
        private object input;

        private Type ClockType;
        private Type InputType;
        private Type RecorderType;

        int frameRate = 30;
        int videoBitRate = 20000;

        public ConfigEntry<KeyCode> VideoRecordHotkey;

        private bool isRecording;

        public VideoTool()
        {
            VideoRecordHotkey = XYTool.Inst.Config.Bind<KeyCode>("VideoTool", "VideoRecordHotkey", KeyCode.G, "VideoTool.RecordHotkey".Translate());
            LoadLib();
        }

        /// <summary>
        /// 加载库文件
        /// </summary>
        public void LoadLib()
        {
            // 检查视频库文件是否存在
            if (File.Exists($"{Paths.ManagedPath}/../Plugins/x86_64/NatCorder.dll"))
            {
                // 加载dll
                try
                {
                    var fileBytes = ResourceUtils.GetEmbeddedResource("NatSuiteRecordersDLL");
                    NatSuite = AppDomain.CurrentDomain.Load(fileBytes);
                    if (NatSuite == null)
                    {
                        throw new NullReferenceException($"NatSuite is null");
                    }
                    ClockType = NatSuite.GetType("NatSuite.Recorders.Clocks.RealtimeClock");
                    InputType = NatSuite.GetType("NatSuite.Recorders.Inputs.CameraInput");
                    RecorderType = NatSuite.GetType("NatSuite.Recorders.MP4Recorder");
                    state = VedioToolState.OK;
                }
                catch (Exception ex)
                {
                    exLog += ex.ToString();
                    state = VedioToolState.HasException;
                }
            }
            else
            {
                state = VedioToolState.NeedInstallLib;
            }
        }

        /// <summary>
        /// 安装视频库文件
        /// </summary>
        public void InstallLib()
        {
            try
            {
                var fileBytes = ResourceUtils.GetEmbeddedResource("NatCorderLibDLL");
                File.WriteAllBytes($"{Paths.ManagedPath}/../Plugins/x86_64/NatCorder.dll", fileBytes);
                state = VedioToolState.NeedReset;
            }
            catch (Exception ex)
            {
                exLog += ex.ToString();
                state = VedioToolState.HasException;
            }
        }

        public void OnGUI()
        {
            switch (state)
            {
                case VedioToolState.NeedInstallLib:
                    GUILayout.Label("VideoTool.NoVideoLib".Translate());
                    if (GUILayout.Button("VideoTool.InstallVideoLib".Translate()))
                    {
                        InstallLib();
                    }
                    break;
                case VedioToolState.NeedReset:
                    GUILayout.Label("VideoTool.NeedReset".Translate());
                    break;
                case VedioToolState.HasException:
                    GUILayout.Label($"{"Common.HasException".Translate()}:{exLog}");
                    break;
                case VedioToolState.OK:
                    if (XYTool.Inst.PhotoBoothVM == null || !XYTool.Inst.PhotoBoothVM.IsActive)
                    {
                        GUILayout.Label("VideoTool.MustPhotoBooth".Translate());
                    }
                    else
                    {
                        int width = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.HorizontalResolution;
                        int height = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.VerticalResolution;
                        if (width % 2 != 0 || height % 2 != 0)
                        {
                            GUILayout.Label(string.Format("VideoTool.ResolutionError".Translate(), width, height));
                        }
                        else
                        {
                            GUILayout.Label(string.Format("VideoTool.RecordResolution".Translate(), width, height));
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("VideoTool.FrameRate".Translate());
                            GUILayout.FlexibleSpace();
                            frameRate = GUIHelper.IntTextGUI(frameRate, "frameRate", 200, 1, 240);
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("VideoTool.BitRate".Translate());
                            GUILayout.FlexibleSpace();
                            videoBitRate = GUIHelper.IntTextGUI(videoBitRate, "videoBitRate", 200, 1000, 1000000);
                            GUILayout.EndHorizontal();
                            if (isRecording)
                            {
                                GUILayout.Label("VideoTool.Recording".Translate());
                                if (GUILayout.Button($"{"VideoTool.StopRecord".Translate()}({VideoRecordHotkey.Value})"))
                                {
                                    StopRecord();
                                }
                            }
                            else
                            {
                                if (GUILayout.Button($"{"VideoTool.StartRecord".Translate()}({VideoRecordHotkey.Value})"))
                                {
                                    StartRecord();
                                }
                            }
                            GUILayout.FlexibleSpace();
                            GUILayout.BeginVertical(GUI.skin.box);
                            GUILayout.Label("VideoTool.Tips".Translate());
                            GUILayout.EndVertical();
                            if (GUILayout.Button("VideoTool.OpenOutputDir".Translate()))
                            {
                                System.Diagnostics.Process.Start($"{VRoid.Studio.Saving.SavingManager.Instance.PathInfo.CustomItemBaseDirectoryPath}/..");
                            }
                        }
                    }
                    break;
            }
        }

        public void Update()
        {
            if (state == VedioToolState.OK)
            {
                if (XYTool.Inst.PhotoBoothVM == null || !XYTool.Inst.PhotoBoothVM.IsActive)
                {
                    if (isRecording)
                    {
                        StopRecord();
                    }
                }
                if (Input.GetKeyDown(VideoRecordHotkey.Value))
                {
                    int width = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.HorizontalResolution;
                    int height = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.VerticalResolution;
                    if (width % 2 != 0 || height % 2 != 0)
                    {
                        Debug.LogWarning(string.Format("VideoTool.ResolutionError".Translate(), width, height));
                    }
                    else
                    {
                        if (isRecording)
                        {
                            StopRecord();
                        }
                        else
                        {
                            StartRecord();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开始录制
        /// </summary>
        public void StartRecord()
        {
            try
            {
                Debug.Log($"开始录制");
                // 帧率
                int width = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.HorizontalResolution;
                int height = XYTool.Inst.PhotoBoothVM.CaptureSizeSetting.VerticalResolution;
                var cam = XYTool.Inst.PhotoBoothVM.PhotoBoothCamera;
                // 创建时钟
                object[] clockParam = new object[2];
                clockParam[0] = frameRate;
                clockParam[1] = true;
                clock = Activator.CreateInstance(ClockType);
                // 创建录制器
                object[] recorderParam = new object[8];
                recorderParam[0] = width;
                recorderParam[1] = height;
                recorderParam[2] = frameRate;
                recorderParam[3] = 0;
                recorderParam[4] = 0;
                recorderParam[5] = videoBitRate * 1000;
                recorderParam[6] = 2;
                recorderParam[7] = 64000;
                recorder = Activator.CreateInstance(RecorderType, recorderParam);
                // 创建录制器的输入
                object[] inputParam = new object[3];
                inputParam[0] = recorder;
                inputParam[1] = clock;
                inputParam[2] = cam;
                input = Activator.CreateInstance(InputType, inputParam);
                isRecording = true;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        /// <summary>
        /// 结束录制
        /// </summary>
        public void StopRecord()
        {
            try
            {
                Debug.Log("结束录制");
                input.GetType().GetMethod("Dispose").Invoke(input, null);
                recorder.GetType().GetMethod("FinishWriting").Invoke(recorder, null);
                isRecording = false;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }
    }

    public enum VedioToolState
    {
        NeedInstallLib,
        NeedReset,
        HasException,
        OK
    }
}
