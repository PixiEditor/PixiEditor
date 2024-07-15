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
        public IColorFilterImplementation ColorFilterImplementation { get; set; }
        public IShaderImplementation ShaderImplementation { get; set; }

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl);
            ImageImplementation = imgImpl;
            
            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;
            
            SkiaShaderImplementation shader = new SkiaShaderImplementation();
            ShaderImplementation = shader;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl, shader);
            PaintImplementation = paintImpl;
            
            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;
            
            MatrixImplementation = new SkiaMatrixImplementation();
            
            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;
            
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl);
            BitmapImplementation = bitmapImpl;
            
            SkiaCanvasImplementation canvasImpl = new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl);
            
            var surfaceImpl = new SkiaSurfaceImplementation(pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImplementation(surfaceImpl);
            imgImpl.SetSurfaceImplementation(surfaceImpl);

            CanvasImplementation = canvasImpl;

            SurfaceImplementation = surfaceImpl;

        }
        
        public void Setup()
        {
            
        }
    }
}
