using System;
using HarmonyLib;
using UnityEngine;
using VRoidStudio.PhotoBooth;
using System.Collections.Generic;

namespace VRoidXYTool
{
    /// <summary>
    /// 姿势数据
    /// </summary>
    public class PoseData
    {
        private static Type FloatType = typeof(float);
        private static Type Vector3Type = typeof(Vector3);
        private static Type QuaternionType = typeof(Quaternion);
        private static Dictionary<string, Type> ISerializedPoseGizmoDefinitionTypeDict;

        public Dictionary<string, ISerializedPoseGizmoDefinitionData> data;

        public PoseData()
        {
        }

        public PoseData(Dictionary<string, ISerializedPoseGizmoDefinition> dict)
        {
            data = new Dictionary<string, ISerializedPoseGizmoDefinitionData>();
            foreach (var kv in dict)
            {
                ISerializedPoseGizmoDefinitionData d = new ISerializedPoseGizmoDefinitionData(kv.Value);
                data[kv.Key] = d;
            }
        }

        public Dictionary<string, ISerializedPoseGizmoDefinition> ToSerializedPose()
        {
            InitTypes();
            Dictionary<string, ISerializedPoseGizmoDefinition> dict = new Dictionary<string, ISerializedPoseGizmoDefinition>();
            foreach (var kv in data)
            {
                try
                {
                    // 通过反射创建对象并赋值
                    Type type = ISerializedPoseGizmoDefinitionTypeDict[kv.Value.TypeName];
                    var obj = AccessTools.CreateInstance(type);
                    type.GetProperty("Name").SetValue(obj, kv.Value.Name);
                    var ps = type.GetProperties();
                    foreach (var p in ps)
                    {
                        if (p.PropertyType == FloatType)
                        {
                            if (kv.Value.FloatDict.ContainsKey(p.Name))
                            {
                                p.SetValue(obj, kv.Value.FloatDict[p.Name]);
                            }
                        }
                        else if (p.PropertyType == Vector3Type)
                        {
                            if (kv.Value.Vector3Dict.ContainsKey(p.Name))
                            {
                                p.SetValue(obj, kv.Value.Vector3Dict[p.Name].ToVector3());
                            }
                        }
                        else if (p.PropertyType == QuaternionType)
                        {
                            if (kv.Value.QuaternionDict.ContainsKey(p.Name))
                            {
                                p.SetValue(obj, kv.Value.QuaternionDict[p.Name].ToQuaternion());
                            }
                        }
                    }
                    dict[kv.Value.Name] = obj as ISerializedPoseGizmoDefinition;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
            return dict;
        }

        public static void InitTypes()
        {
            if (ISerializedPoseGizmoDefinitionTypeDict == null)
            {
                ISerializedPoseGizmoDefinitionTypeDict = new Dictionary<string, Type>();
                var types = TypeHelper.GetInterfaceChildClass(typeof(ISerializedPoseGizmoDefinition));
                foreach (var type in types)
                {
                    ISerializedPoseGizmoDefinitionTypeDict[type.Name] = type;
                }
            }
        }
    }

    public class ISerializedPoseGizmoDefinitionData
    {
        private static Type FloatType = typeof(float);
        private static Type Vector3Type = typeof(Vector3);
        private static Type QuaternionType = typeof(Quaternion);
        public string TypeName;
        public string Name;
        public Dictionary<string, float> FloatDict;
        public Dictionary<string, V3> Vector3Dict;
        public Dictionary<string, QuaternionData> QuaternionDict;

        public ISerializedPoseGizmoDefinitionData()
        {
        }

        public ISerializedPoseGizmoDefinitionData(ISerializedPoseGizmoDefinition definition)
        {
            Name = definition.Name;
            Type type = definition.GetType();
            TypeName = type.Name;
            FloatDict = new Dictionary<string, float>();
            Vector3Dict = new Dictionary<string, V3>();
            QuaternionDict = new Dictionary<string, QuaternionData>();
            // 通过反射将数据取出并存储
            var ps = type.GetProperties();
            foreach (var p in ps)
            {
                if (p.PropertyType == FloatType)
                {
                    FloatDict[p.Name] = (float)p.GetValue(definition);
                }
                if (p.PropertyType == Vector3Type)
                {
                    Vector3Dict[p.Name] = new V3((Vector3)p.GetValue(definition));
                }
                if (p.PropertyType == QuaternionType)
                {
                    QuaternionDict[p.Name] = new QuaternionData((Quaternion)p.GetValue(definition));
                }
            }
        }
    }
}
