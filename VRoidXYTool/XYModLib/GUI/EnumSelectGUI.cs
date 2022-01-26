using System;
using UnityEngine;

namespace XYModLib
{
    public class EnumSelectGUI
    {
        private Type EnumType;
        private string[] EnumNames;
        private string name;
        public int NowSelectedIndex;

        /// <summary>
        /// 竖型GUI时的宽度
        /// </summary>
        public int VerticalWidth = 100;

        /// <summary>
        /// 是否忽略GUI皮肤
        /// </summary>
        public bool HideSkin;

        public EnumSelectGUI(Type e, string guiName)
        {
            name = guiName;
            EnumType = e;
            EnumNames = Enum.GetNames(e);
        }

        public void VerticalGUI()
        {
            if (HideSkin)
            {
                GUILayout.BeginVertical(GUILayout.Width(VerticalWidth), GUILayout.ExpandHeight(true));
            }
            else
            {
                GUILayout.BeginVertical(name, GUI.skin.box, GUILayout.Width(VerticalWidth), GUILayout.ExpandHeight(true));
            }
            GUILayout.Space(16);
            NowSelectedIndex = GUILayout.SelectionGrid(NowSelectedIndex, EnumNames, 1);
            GUILayout.EndVertical();
        }

        public void HorizontalGUI()
        {
            if (HideSkin)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
            }
            NowSelectedIndex = GUILayout.SelectionGrid(NowSelectedIndex, EnumNames, EnumNames.Length);
            GUILayout.EndHorizontal();
        }
    }
}
