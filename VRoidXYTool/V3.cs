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

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static V3 Parse(Vector3 vector)
        {
            return new V3() { x = vector.x, y = vector.y, z = vector.z };
        }
    }
}
