using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BepInEx.Configuration;

namespace VRoidXYTool
{
    /// <summary>
    /// 线框工具
    /// </summary>
    public class WireframeTool
    {
        public GameObject Wireframe;
        private bool open;

        public bool Open
        {
            get
            {
                return open;
            }
            set
            {
                if (open == value) return;
                open = value;
                Wireframe.SetActive(open);
            }
        }

        public WireframeTool()
        {
            Wireframe = new GameObject("WireframeBehaviour");
            Wireframe.AddComponent<WireframeBehaviour>();
            GameObject.DontDestroyOnLoad(Wireframe);
            Wireframe.SetActive(false);
        }

        public void Update()
        {
        }

        public void OnGUI()
        {
            Open = GUILayout.Toggle(Open, "WireframeTool.WireframeMode".Translate());
        }
    }

    public class WireframeBehaviour : MonoBehaviour
    {
        private bool open;

        public bool Enabled
        {
            get
            {
                return open;
            }
            set
            {
                if (open != value)
                {
                    open = value;
                    if (value)
                    {
                        Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRender));
                        Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnPostRender));
                        return;
                    }
                    Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(OnPreRender));
                    Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnPostRender));
                    GL.wireframe = false;
                    foreach (KeyValuePair<Camera, CameraClearFlags> keyValuePair in _origFlags)
                    {
                        if (keyValuePair.Key != null)
                        {
                            keyValuePair.Key.clearFlags = keyValuePair.Value;
                        }
                    }
                    _origFlags.Clear();
                }
            }
        }

        public void OnEnable()
        {
            Enabled = true;
        }

        public void OnDisable()
        {
            Enabled = false;
        }

        private static void OnPreRender(Camera cam)
        {
            if (cam.name != "NormalLayerCamera") return;
            if (GL.wireframe)
            {
                return;
            }
            if (!_origFlags.ContainsKey(cam))
            {
                _origFlags.Add(cam, cam.clearFlags);
            }
            cam.clearFlags = CameraClearFlags.Color;
            GL.wireframe = true;
        }

        private static void OnPostRender(Camera cam)
        {
            CameraClearFlags cameraClearFlags;
            if (_origFlags.TryGetValue(cam, out cameraClearFlags))
            {
                cam.clearFlags = cameraClearFlags;
                GL.wireframe = false;
            }
        }

        private static readonly Dictionary<Camera, CameraClearFlags> _origFlags = new Dictionary<Camera, CameraClearFlags>();
    }
}