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

        public static void SaveJson()
        {
            var json = JsonConvert.SerializeObject(XYTool.Inst.MainVM.CurrentFile.Engine.Context.ActiveModel.BaseCollection);
            File.WriteAllText($"{Paths.GameRootPath}/Test.json", json);
        }

        public static void CheckAsset()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var assets = Resources.LoadAll<TextAsset>("vroidcore");
            foreach (var asset in assets)
            {
                
            }
            sw.Stop();
            Debug.Log($"找到{assets.Length}个资源，共耗时:{sw.ElapsedMilliseconds}ms");
        }

        public static void DumpModel()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var Transferables = XYTool.Inst.CurrentFileM.engine.Context.ActiveModel.Transferables;
            //string json = JsonConvert.SerializeObject(Transferables, Formatting.Indented);
            //File.WriteAllText($"{Paths.GameRootPath}/Test.json", json);
            Dictionary<string, VRoidCore.Protobuf.Transferables.Transferable> dict = new Dictionary<string, VRoidCore.Protobuf.Transferables.Transferable>();
            foreach (var transferable in Transferables)
            {
                dict.Add(transferable.Key, transferable.Value.ToProtobuf(transferable.Key));
            }
            Debug.Log($"保存数据到json");
            string json = JsonConvert.SerializeObject(dict, Formatting.None);
            File.WriteAllText($"{Paths.GameRootPath}/Test.json", json);
            sw.Stop();
            Debug.Log($"耗时{sw.ElapsedMilliseconds}ms");
        }

        public static void DumpModel2()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var datas = XYTool.Inst.CurrentFileM.engine.Context.ActiveModel.Transferables;
            Debug.Log($"开始输出");
            foreach (var data in datas)
            {
                Debug.Log($"{data.Value.TypeId}");
                string json = JsonConvert.SerializeObject(data.Value.ToProtobuf(data.Key));
                File.WriteAllText($"{Paths.GameRootPath}/Test/{data.Value.TypeId}.json", json);
            }
            sw.Stop();
            Debug.Log($"耗时{sw.ElapsedMilliseconds}ms");
        }

        public static void LoadModel()
        {
            var json = File.ReadAllText($"{Paths.GameRootPath}/Test/TransferableType.N00.Breast.json");
            var tdata = JsonConvert.DeserializeObject<VRoidCore.Protobuf.Transferables.Transferable>(json);
            var t = new VRoidCore.Common.TransferableTypes.Transferable();
            t.FromProtobuf(tdata);

            var datas = XYTool.Inst.CurrentFileM.engine.Context.ActiveModel.Transferables;
            foreach (var data in datas)
            {
                if (data.Value.TypeId == "TransferableType.N00.Breast")
                {
                    Debug.Log($"找到了TransferableType.N00.Breast，进行数据替换");
                    Debug.Log($"原数据:\n{JsonConvert.SerializeObject(data.Value.Collection.Global)}");
                    Debug.Log($"替换数据:\n{JsonConvert.SerializeObject(t.Collection.Global)}");
                    data.Value.Collection.Global = t.Collection.Global;
                    //data.Value.FromProtobuf(tdata);
                }
            }
            // 模型刷新了，但是UI上的滑条还没有刷新
            XYTool.Inst.CurrentFileM.engine.Context.MarkNeedsUpdate(true);
        }
    }
}
