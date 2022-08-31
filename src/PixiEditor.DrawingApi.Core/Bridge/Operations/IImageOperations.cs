using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.DrawingApi.Core.Bridge.Operations
{
    public interface IImageOperations
    {
        public Image Snapshot(DrawingSurface drawingSurface);
        public void DisposeImage(Image image);
        public Image FromEncodedData(string path);
    }
}
