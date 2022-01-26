using System;
using System.Linq;
using VRoidCore.Common.SpecificTypes;
using VRoid.Studio.TextureEditor.Layer.ViewModel;

namespace VRoidXYTool
{
    /// <summary>
    /// 链接纹理数据
    /// </summary>
    public class LinkTexture
    {
        public RasterLayerViewModel layer;
        public DateTime LastWriteTime;
        public bool CanExportUV;

        public LinkTexture(RasterLayerViewModel vm)
        {
            layer = vm;
            LastWriteTime = DateTime.Now;
            TexturePath referringTexturePath = vm._parent.ReferringTexturePaths.FirstOrDefault<TexturePath>();
            if (referringTexturePath != null && vm.featureViewModel.actionHandler.IsGuideExportable(referringTexturePath))
            {
                CanExportUV = true;
            }
        }

        /// <summary>
        /// 纹理是否存在
        /// </summary>
        /// <returns></returns>
        public bool IsVaild()
        {
            if (layer == null)
            {
                return false;
            }
            try
            {
                string name = layer.TranslatedDisplayName;
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
