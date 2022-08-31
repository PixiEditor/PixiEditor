using PixiEditor.DrawingApi.Core.Bridge.Operations;

namespace PixiEditor.DrawingApi.Core.Bridge
{
    public interface IDrawingBackend
    {
        public void Setup();
        public IDrawingBackendColorOperations ColorOperations { get; }
        public IImageOperations ImageOperations { get; }
        public ICanvasOperations CanvasOperations { get; }
        public IPaintOperations PaintOperations { get; set; }
    }
}
