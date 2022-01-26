using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace VRoidXYTool
{
    public static class TypeHelper
    {
        /// <summary>
        /// 获取接口的所有实现类
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="allAssembly">是否在所有程序集内搜索</param>
        /// <returns></returns>
        public static List<Type> GetInterfaceChildClass(Type interfaceType, bool allAssembly = false)
        {
            List<Type> result = new List<Type>();
            Type[] types;
            if (allAssembly)
            {
                types = AccessTools.AllTypes().ToArray();
            }
            else
            {
                types = interfaceType.Assembly.GetTypes();
            }
            foreach (var type in types)
            {
                if (type.IsInterface) continue;
                var interfaces = type.GetInterfaces();
                foreach (var iface in interfaces)
                {
                    if (iface == interfaceType)
                    {
                        result.Add(type);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
