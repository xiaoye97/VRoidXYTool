using System;

namespace VRoidXYTool
{
    /// <summary>
    /// 相机位置预设数据
    /// </summary>
    [Serializable]
    public class CameraPosPresetData
    {
        public static int PresetCount = 10;
        public PerspectiveCameraPosPreset[] PerspectiveCameraPosPresets;
        public OrthographicCameraPosPreset[] OrthographicCameraPosPresets;
    }

    /// <summary>
    /// 透视相机位置预设
    /// </summary>
    [Serializable]
    public class PerspectiveCameraPosPreset
    {
        public V3 Pos;
        public V3 Rot;
    }

    /// <summary>
    /// 正交相机位置预设
    /// </summary>
    [Serializable]
    public class OrthographicCameraPosPreset
    {
        public V3 Pos;
        public V3 Rot;
        public float OrthographicSize;
    }
}
