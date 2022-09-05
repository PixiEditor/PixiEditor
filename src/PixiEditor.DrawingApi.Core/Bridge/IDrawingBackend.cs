using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Bridge.Operations;

namespace PixiEditor.DrawingApi.Core.Bridge
{
    public interface IDrawingBackend
    {
        public void Setup();
        public IDrawingBackendColorOperations ColorOperations { get; }
        public IImageOperations ImageOperations { get; }
        public ICanvasOperations CanvasOperations { get; }
        public IPaintImplementation PaintImplementation { get; set; }
        public IVectorPathImplementation PathImplementation { get; set; }
        public IMatrix3X3Implementation MatrixImplementation { get; set; }
        public IPixmapImplementation PixmapImplementation { get; set; }
    }
}
