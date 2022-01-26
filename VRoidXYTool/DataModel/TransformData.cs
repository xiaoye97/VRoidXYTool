using UnityEngine;

namespace VRoidXYTool
{
    public class TransformData
    {
        public V3 Position;
        public V3 Rotation;
        public V3 Scale;

        public TransformData()
        {
            Position = new V3();
            Rotation = new V3();
            Scale = new V3();
        }

        public TransformData(Transform transform)
        {
            FromTransform(transform);
        }

        public void FromTransform(Transform transform)
        {
            Position = new V3(transform.position);
            Rotation = new V3(transform.localEulerAngles);
            Scale = new V3(transform.localScale);
        }

        public void Apply(Transform transform)
        {
            transform.position = Position.ToVector3();
            transform.localEulerAngles = Rotation.ToVector3();
            transform.localScale = Scale.ToVector3();
        }
    }
}
