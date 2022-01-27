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

        public Dictionary<string, ISerializedPoseGizmoDefinitionData> Data;
        public Dictionary<string, RollControlHandleData> RollControlHandleData;
        //public Dictionary<string, TransformData> RollControlHandleData;

        public PoseData()
        {
        }

        public PoseData(Dictionary<string, ISerializedPoseGizmoDefinition> dict)
        {
            Data = new Dictionary<string, ISerializedPoseGizmoDefinitionData>();
            foreach (var kv in dict)
            {
                ISerializedPoseGizmoDefinitionData d = new ISerializedPoseGizmoDefinitionData(kv.Value);
                Data[kv.Key] = d;
            }
        }

        /// <summary>
        /// 将数据转换为运行时使用的字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ISerializedPoseGizmoDefinition> ToSerializedPose()
        {
            InitTypes();
            Dictionary<string, ISerializedPoseGizmoDefinition> dict = new Dictionary<string, ISerializedPoseGizmoDefinition>();
            foreach (var kv in Data)
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
                    var fields = type.GetFields(AccessTools.all);
                    foreach (var f in fields)
                    {
                        if (!f.IsStatic && !f.Name.Contains("<"))
                        {
                            if (f.FieldType == FloatType)
                            {
                                if (kv.Value.FloatDict.ContainsKey(f.Name))
                                {
                                    f.SetValue(obj, kv.Value.FloatDict[f.Name]);
                                }
                            }
                            if (f.FieldType == Vector3Type)
                            {
                                if (kv.Value.Vector3Dict.ContainsKey(f.Name))
                                {
                                    f.SetValue(obj, kv.Value.Vector3Dict[f.Name].ToVector3());
                                }
                            }
                            if (f.FieldType == QuaternionType)
                            {
                                if (kv.Value.QuaternionDict.ContainsKey(f.Name))
                                {
                                    f.SetValue(obj, kv.Value.QuaternionDict[f.Name].ToQuaternion());
                                }
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
            var fields = type.GetFields(AccessTools.all);
            foreach (var f in fields)
            {
                if (!f.IsStatic && !f.Name.Contains("<"))
                {
                    if (f.FieldType == FloatType)
                    {
                        FloatDict[f.Name] = (float)f.GetValue(definition);
                    }
                    if (f.FieldType == Vector3Type)
                    {
                        Vector3Dict[f.Name] = new V3((Vector3)f.GetValue(definition));
                    }
                    if (f.FieldType == QuaternionType)
                    {
                        QuaternionDict[f.Name] = new QuaternionData((Quaternion)f.GetValue(definition));
                    }
                }
            }
        }
    }

    public class RollControlHandleData
    {
        public V3 localCurrentPoint;
        public V3 localStartPoint;
        public TransformData Collider0Transform;
        public TransformData Collider1Transform;
    }
}
