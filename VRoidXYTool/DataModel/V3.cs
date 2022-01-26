using UnityEngine;

namespace VRoidXYTool
{
    public class V3
    {
        public float x, y, z;

        public V3()
        {
        }

        public V3 (float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public V3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
