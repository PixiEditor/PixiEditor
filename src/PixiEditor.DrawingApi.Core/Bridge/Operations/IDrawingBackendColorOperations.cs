using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface IDrawingBackendColorOperations
    {
        public ColorF ColorToColorF(uint colorValue);
        public Color ColorFToColor(ColorF color);
    }
}
