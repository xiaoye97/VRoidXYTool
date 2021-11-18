using UnityEngine;

namespace VRoidXYTool
{
    public static class PosHelper
    {
        /// <summary>
        /// 获取头的位置
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetHeadPos()
        {
            GameObject headFront = GameObject.Find("J_Adj_C_HeadFront");
            GameObject headBack = GameObject.Find("J_Adj_C_HeadBack");
            if (headFront != null && headBack != null)
            {
                return (headFront.transform.position + headBack.transform.position) / 2;
            }
            else
            {
                return Vector3.zero;
            }
        }

        /// <summary>
        /// 获取臀部的位置
        /// </summary>
        /// <returns></returns>
        public static Vector3 GetHipsPos()
        {
            GameObject hips = GameObject.Find("J_Bip_C_Hips");
            if (hips != null)
            {
                return hips.transform.position;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}
