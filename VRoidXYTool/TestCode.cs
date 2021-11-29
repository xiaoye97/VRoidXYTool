using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MToon;

namespace VRoidXYTool
{
    public static class TestCode
    {
        public static void ReplaceMat()
        {
            var renders = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
            foreach (var render in renders)
            {
                if (render.material.name.Contains("CLOTH"))
                {
                    Utils.SetRenderMode(render.material, MToon.RenderMode.Transparent, 0, false);
                    Utils.SetCullMode(render.material, CullMode.Off);
                    Debug.Log($"将{render.gameObject.name}的材质切换到半透明模式");
                }
            }
        }
    }
}
