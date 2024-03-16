using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Skia.Implementations;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia
{
    public class SkiaDrawingBackend : IDrawingBackend
    {
        public SkiaColorImplementation ColorImplementation { get; }
        public SkiaImageImplementation ImageImplementation { get; }
        public SkiaImgDataImplementation ImgDataImplementation { get; }
        public SkiaCanvasImplementation CanvasImplementation { get; }
        public SkiaPaintImplementation PaintImplementation { get; }
        public SkiaPathImplementation PathImplementation { get; }
        public SkiaMatrixImplementation MatrixImplementation { get; }
        public SkiaPixmapImplementation PixmapImplementation { get; }
        public SkiaSurfaceImplementation SurfaceImplementation { get; }
        public SkiaColorSpaceImplementation ColorSpaceImplementation { get; }
        public SkiaBitmapImplementation BitmapImplementation { get; }
        public SkiaColorFilterImplementation ColorFilterImplementation { get; set; }
        IColorImplementation IDrawingBackend.ColorImplementation => ColorImplementation;
        IImageImplementation IDrawingBackend.ImageImplementation => ImageImplementation;
        IImgDataImplementation IDrawingBackend.ImgDataImplementation => ImgDataImplementation;
        ICanvasImplementation IDrawingBackend.CanvasImplementation => CanvasImplementation;
        IPaintImplementation IDrawingBackend.PaintImplementation => PaintImplementation;
        IVectorPathImplementation IDrawingBackend.PathImplementation => PathImplementation;
        IMatrix3X3Implementation IDrawingBackend.MatrixImplementation => MatrixImplementation;
        IPixmapImplementation IDrawingBackend.PixmapImplementation => PixmapImplementation;
        ISurfaceImplementation IDrawingBackend.SurfaceImplementation => SurfaceImplementation;
        IColorSpaceImplementation IDrawingBackend.ColorSpaceImplementation => ColorSpaceImplementation;
        IBitmapImplementation IDrawingBackend.BitmapImplementation => BitmapImplementation;

        IColorFilterImplementation IDrawingBackend.ColorFilterImplementation
        {
            get => ColorFilterImplementation;
            set => ColorFilterImplementation = (SkiaColorFilterImplementation)value;
        }

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl);
            ImageImplementation = imgImpl;
            
            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl);
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
