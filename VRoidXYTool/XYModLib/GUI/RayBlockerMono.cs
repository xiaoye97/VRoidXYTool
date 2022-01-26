using System;
using UnityEngine;

namespace XYModLib
{
    public class RayBlockerMono : MonoBehaviour
    {
        public Action OnUpdate;
        void Update()
        {
            if (OnUpdate != null)
                OnUpdate();
        }
    }
}
