using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRoidXYTool
{
    /// <summary>
    /// 参考物体
    /// </summary>
    public class GuideObject
    {
        /// <summary>
        /// 是否有效
        /// </summary>
        public bool IsVaild;

        public string GuideName;

        public GuideObjectType ObjectType;

        public GameObject GO;

        /// <summary>
        /// 图片数据，仅在图片类型下存在
        /// </summary>
        public GuideImageData ImageData;
    }

    public enum GuideObjectType
    {
        None,
        Image,
        Model
    }
}
