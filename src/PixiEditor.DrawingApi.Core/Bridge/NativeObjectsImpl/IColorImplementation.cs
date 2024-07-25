using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl
{
    public interface IColorImplementation
    {
        public ColorF ColorToColorF(uint colorValue);
        public Color ColorFToColor(ColorF color);
        public ColorType GetPlatformColorType();
    }
}
