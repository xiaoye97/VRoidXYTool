using System.Linq;
using UnityEngine;
using VRoid.UI.Component;

namespace VRoidXYTool
{
    public class CameraTool
    {
        private MultipleRenderTextureCamera MRTcamera;
        private Camera MainCamera;

        public void OnGUI()
        {
            GUILayout.BeginVertical("相机工具", GUI.skin.window);
            if (MRTcamera == null || MainCamera == null)
            {
                if (GUILayout.Button("查找相机"))
                {
                    FindCamera();
                }
            }
            else
            {
                if (MainCamera.orthographic)
                {
                    OrthoGUI();
                }
                else
                {
                    NormalGUI();
                }
            }
            GUILayout.EndVertical();
        }

        public void FindCamera()
        {
            MRTcamera = GameObject.FindObjectOfType<MultipleRenderTextureCamera>();
            if (MRTcamera != null)
            {
                MainCamera = MRTcamera._instantiatedCameras.Keys.First().PrimaryCamera;
            }
        }

        public void SetCameraPos(Vector3 pos, Vector3 rot)
        {
            MRTcamera.transform.position = pos;
            MRTcamera.transform.localEulerAngles = rot;
        }

        /// <summary>
        /// 透视模式下GUI
        /// </summary>
        public void NormalGUI()
        {
            if (GUILayout.Button("设置相机为正交模式"))
            {
                MainCamera.orthographic = true;
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginHorizontal("相机位置(全身)", GUI.skin.window);
            if (GUILayout.Button("前"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[0], GetAroundAngles()[0]);
            }
            if (GUILayout.Button("后"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[1], GetAroundAngles()[1]);
            }
            if (GUILayout.Button("左"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[2], GetAroundAngles()[2]);
            }
            if (GUILayout.Button("右"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 4)[3], GetAroundAngles()[3]);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("相机位置(头部)", GUI.skin.window);
            if (GUILayout.Button("前"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[0], GetAroundAngles()[0]);
            }
            if (GUILayout.Button("后"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[1], GetAroundAngles()[1]);
            }
            if (GUILayout.Button("左"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[2], GetAroundAngles()[2]);
            }
            if (GUILayout.Button("右"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[3], GetAroundAngles()[3]);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 正交模式下GUI
        /// </summary>
        public void OrthoGUI()
        {
            if (GUILayout.Button("设置相机为透视模式"))
            {
                MainCamera.orthographic = false;
            }
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("正交尺寸", GUI.skin.window);
            GUILayout.Label($"{MainCamera.orthographicSize}");
            MainCamera.orthographicSize = GUILayout.HorizontalSlider(MainCamera.orthographicSize, 0.1f, 2f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.15")) MainCamera.orthographicSize = 0.15f;
            if (GUILayout.Button("0.2")) MainCamera.orthographicSize = 0.2f;
            if (GUILayout.Button("0.3")) MainCamera.orthographicSize = 0.3f;
            if (GUILayout.Button("0.8")) MainCamera.orthographicSize = 0.8f;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal("相机位置(全身)", GUI.skin.window);
            if (GUILayout.Button("前"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[0], GetAroundAngles()[0]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("后"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[1], GetAroundAngles()[1]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("左"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[2], GetAroundAngles()[2]);
                MainCamera.orthographicSize = 0.8f;
            }
            if (GUILayout.Button("右"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHipsPos(), 1)[3], GetAroundAngles()[3]);
                MainCamera.orthographicSize = 0.8f;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("相机位置(头部)", GUI.skin.window);
            if (GUILayout.Button("前"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[0], GetAroundAngles()[0]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("后"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[1], GetAroundAngles()[1]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("左"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[2], GetAroundAngles()[2]);
                MainCamera.orthographicSize = 0.15f;
            }
            if (GUILayout.Button("右"))
            {
                SetCameraPos(GetAroundPos(PosHelper.GetHeadPos(), 1)[3], GetAroundAngles()[3]);
                MainCamera.orthographicSize = 0.15f;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 根据中心和半径获取点四周的位置
        /// 以Z轴作为前方
        /// </summary>
        /// <param name="center">中心点</param>
        /// <param name="radius">半径</param>
        /// <returns>四周的位置 前后左右</returns>
        public static Vector3[] GetAroundPos(Vector3 center, float radius)
        {
            Vector3[] result = new Vector3[4];
            result[0] = new Vector3(center.x, center.y, center.z + radius);
            result[1] = new Vector3(center.x, center.y, center.z - radius);
            result[2] = new Vector3(center.x - radius, center.y, center.z);
            result[3] = new Vector3(center.x + radius, center.y, center.z);
            return result;
        }

        /// <summary>
        /// 获得四周的角度
        /// 以Z轴为前方
        /// </summary>
        /// <returns></returns>
        public static Vector3[] GetAroundAngles()
        {
            Vector3[] result = new Vector3[4];
            result[0] = new Vector3(0, 180, 0);
            result[1] = Vector3.zero;
            result[2] = new Vector3(0, 90, 0);
            result[3] = new Vector3(0, 270, 0);
            return result;
        }
    }
}