using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Bridge.Operations;
using PixiEditor.DrawingApi.Skia.Exceptions;
using PixiEditor.DrawingApi.Skia.Implementations;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia
{
    public class SkiaDrawingBackend : IDrawingBackend
    {
        public GRContext? GraphicsContext
        {
            get => _grContext;
            set
            {
                if (_grContext != null)
                {
                    throw new GrContextAlreadyInitializedException();
                }
                
                _grContext = value;
            }
        }
        
        public bool IsHardwareAccelerated => GraphicsContext != null;
        
        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        ISurfaceImplementation IDrawingBackend.SurfaceImplementation => SurfaceImplementation;
        public SkiaSurfaceImplementation SurfaceImplementation { get; }
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        public IColorFilterImplementation ColorFilterImplementation { get; }
        public IImageFilterImplementation ImageFilterImplementation { get; }
        public IShaderImplementation ShaderImplementation { get; set; }

        private GRContext _grContext;

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;

            SkiaImageFilterImplementation imageFilterImpl = new SkiaImageFilterImplementation();
            ImageFilterImplementation = imageFilterImpl;
            
            SkiaShaderImplementation shader = new SkiaShaderImplementation();
            ShaderImplementation = shader;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl, imageFilterImpl, shader);
            PaintImplementation = paintImpl;
            
            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;
            
            MatrixImplementation = new SkiaMatrixImplementation();
            
            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl, pixmapImpl);
            ImageImplementation = imgImpl;
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl, pixmapImpl);
            BitmapImplementation = bitmapImpl;
            
            SkiaCanvasImplementation canvasImpl = new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl);
            
            SurfaceImplementation = new SkiaSurfaceImplementation(GraphicsContext, pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImplementation(SurfaceImplementation);
            imgImpl.SetSurfaceImplementation(SurfaceImplementation);

            CanvasImplementation = canvasImpl;
        }
        
        public void Setup()
        {
            SurfaceImplementation.GrContext = GraphicsContext;
        }
    }
}
