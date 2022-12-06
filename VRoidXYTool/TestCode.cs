using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MToon;
using Newtonsoft.Json;
using System.IO;
using BepInEx;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace VRoidXYTool
{
    public static class TestCode
    {
        //public static void ReplaceMat()
        //{
        //    var renders = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
        //    foreach (var render in renders)
        //    {
        //        if (render.material.name.Contains("CLOTH"))
        //        {
        //            Utils.SetRenderMode(render.material, MToon.RenderMode.Transparent, 0, false);
        //            Utils.SetCullMode(render.material, CullMode.Off);
        //            Debug.Log($"将{render.gameObject.name}的材质切换到半透明模式");
        //        }
        //    }
        //}

        //public static void LogPath()
        //{
        //    var pathInfo = VRoid.Studio.Saving.SavingManager.Instance.PathInfo;
        //    Debug.Log($"ApplicationDirectoryPath:{pathInfo.ApplicationDirectoryPath}");
        //    Debug.Log($"ApplicationPreferencesFilePath:{pathInfo.ApplicationPreferencesFilePath}");
        //    Debug.Log($"AutoCleanupTempDirectoryPath:{pathInfo.AutoCleanupTempDirectoryPath}");
        //    Debug.Log($"AvatarDirectoryPath:{pathInfo.AvatarDirectoryPath}");
        //    Debug.Log($"CustomItemBaseDirectoryPath:{pathInfo.CustomItemBaseDirectoryPath}");
        //    Debug.Log($"HairPresetBaseDirectoryPath:{pathInfo.HairPresetBaseDirectoryPath}");
        //    Debug.Log($"PreferencesDirectoryPath:{pathInfo.PreferencesDirectoryPath}");
        //    Debug.Log($"SampleAvatarDirectoryPath:{pathInfo.SampleAvatarDirectoryPath}");
        //    Debug.Log($"TempAvatarDirectoryBasePath:{pathInfo.TempAvatarDirectoryBasePath}");
        //    Debug.Log($"TempDirectoryBasePath:{pathInfo.TempDirectoryBasePath}");
        //    Debug.Log($"VRoidHubAccountFilePath:{pathInfo.VRoidHubAccountFilePath}");
        //    Debug.Log($"VRoidHubDirectoryPath:{pathInfo.VRoidHubDirectoryPath}");
        //    Debug.Log($"VRoidHubRelationFilePath:{pathInfo.VRoidHubRelationFilePath}");
        //}
    }
}
