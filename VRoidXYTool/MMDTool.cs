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
using VRoidXYTool.MMD;

namespace VRoidXYTool
{
    public class MMDTool
    {
        public string vmdAnimPath = "";
        public string vmdName = "step1";
        private UnityVMDPlayer player = null;
        private GUILayoutOption charWidth = GUILayout.Width(22);

        public MMDTool()
        {

        }

        public void OnGUI()
        {
            if (XYTool.Inst.PhotoBoothVM == null || !XYTool.Inst.PhotoBoothVM.IsActive)
            {
                GUILayout.Label("必须开启摄影棚才能使用此工具");
            }
            else
            {
                if (player == null)
                {
                    if (GUILayout.Button("添加播放器"))
                    {
                        player = XYTool.Inst.PhotoBoothVM.animator.GetComponent<UnityVMDPlayer>();
                        if (player == null)
                        {
                            player = XYTool.Inst.PhotoBoothVM.animator.gameObject.AddComponent<UnityVMDPlayer>();
                        }
                    }
                }
                else
                {
                    if (GUILayout.Button("加载VMD文件"))
                    {
                        SelectVMDFile();
                    }
                    if (player != null && player.VMDReader != null)
                    {
                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label($"当前文件:{vmdAnimPath}");
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical(GUI.skin.box);
                        if (player.VMDReader.FrameCount > 0)
                        {
                            GUILayout.BeginHorizontal();
                            int v = (int)(100f * player.FrameNumber / player.VMDReader.FrameCount);
                            GUILayout.HorizontalSlider(player.FrameNumber, 0, player.VMDReader.FrameCount, GUILayout.Width(200));
                            GUILayout.Space(10);
                            GUILayout.Label($"{player.FrameNumber}/{player.VMDReader.FrameCount} {v}%");
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            if (player.IsPlaying)
                            {
                                if (GUILayout.Button("∥", charWidth))
                                {
                                    player.Pause();
                                }
                            }
                            else
                            {
                                if (player.IsEnd || player.FrameNumber == 0)
                                {
                                    if (GUILayout.Button("▶", charWidth))
                                    {
                                        player.Play(player.VMDReader, 0);
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("▶", charWidth))
                                    {
                                        player.Play();
                                    }
                                }
                            }
                            if (GUILayout.Button("■", charWidth))
                            {
                                player.Stop();
                            }
                            player.IsLoop = GUILayout.Toggle(player.IsLoop, "循环播放");
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Label($"数据异常，无法播放");
                        }
                        GUILayout.EndVertical();
                    }
                }
            }
        }

        public async void SelectVMDFile()
        {
            var result = await FileDialogUtil.OpenFilePanel("VMD", null, FileHelper.GetVMDFilters(), false);
            if (result != null && result.Length > 0)
            {
                vmdAnimPath = result[0];
                await player.LoadForPlay(vmdAnimPath);
            }
        }
    }
}
