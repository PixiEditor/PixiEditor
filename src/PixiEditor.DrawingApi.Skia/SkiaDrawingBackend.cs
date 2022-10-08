using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Skia.Implementations;

namespace PixiEditor.DrawingApi.Skia
{
    public class SkiaDrawingBackend : IDrawingBackend
    {
        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        public ISurfaceImplementation SurfaceImplementation { get; }
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        
        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl);
            ImageImplementation = imgImpl;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation();
            PaintImplementation = paintImpl;
            
            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;
            
            MatrixImplementation = new SkiaMatrixImplementation();
            
            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;
            
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation();
            BitmapImplementation = bitmapImpl;
            
            SkiaCanvasImplementation canvasImpl = new SkiaCanvasImplementation(paintImpl, null, imgImpl, bitmapImpl, pathImpl);
            
            var surfaceImpl = new SkiaSurfaceImplementation(pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImpl(surfaceImpl);

            CanvasImplementation = canvasImpl;

            SurfaceImplementation = surfaceImpl;

        }
        
        public void Setup()
        {
            
        }
    }
}
