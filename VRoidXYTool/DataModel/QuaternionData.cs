using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRoidXYTool
{
    public class QuaternionData
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionData()
        {
        }

        public QuaternionData(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}
