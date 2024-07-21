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
        
        public bool IsGpuAccelerated => GraphicsContext != null;
        
        public IColorImplementation ColorImplementation { get; }
        public IImageImplementation ImageImplementation { get; }
        public IImgDataImplementation ImgDataImplementation { get; }
        public ICanvasImplementation CanvasImplementation { get; }
        public IPaintImplementation PaintImplementation { get; }
        public IVectorPathImplementation PathImplementation { get; }
        public IMatrix3X3Implementation MatrixImplementation { get; }
        public IPixmapImplementation PixmapImplementation { get; }
        public ISurfaceImplementation SurfaceImplementation => _surfaceImplementation;
        public IColorSpaceImplementation ColorSpaceImplementation { get; }
        public IBitmapImplementation BitmapImplementation { get; }
        public IColorFilterImplementation ColorFilterImplementation { get; set; }
        public IShaderImplementation ShaderImplementation { get; set; }
        
        private SkiaSurfaceImplementation _surfaceImplementation;
        private GRContext _grContext;

        public SkiaDrawingBackend()
        {
            ColorImplementation = new SkiaColorImplementation();
            
            SkiaImgDataImplementation dataImpl = new SkiaImgDataImplementation();
            ImgDataImplementation = dataImpl;
            
            SkiaColorSpaceImplementation colorSpaceImpl = new SkiaColorSpaceImplementation();
            ColorSpaceImplementation = colorSpaceImpl;

            SkiaPixmapImplementation pixmapImpl = new SkiaPixmapImplementation(colorSpaceImpl);
            PixmapImplementation = pixmapImpl;
            
            SkiaImageImplementation imgImpl = new SkiaImageImplementation(dataImpl, pixmapImpl);
            ImageImplementation = imgImpl;
            
            SkiaColorFilterImplementation colorFilterImpl = new SkiaColorFilterImplementation();
            ColorFilterImplementation = colorFilterImpl;
            
            SkiaShaderImplementation shaderImpl = new SkiaShaderImplementation();
            ShaderImplementation = shaderImpl;
            
            SkiaPaintImplementation paintImpl = new SkiaPaintImplementation(colorFilterImpl, shaderImpl);
            PaintImplementation = paintImpl;
            
            SkiaPathImplementation pathImpl = new SkiaPathImplementation();
            PathImplementation = pathImpl;
            
            MatrixImplementation = new SkiaMatrixImplementation();
            
            SkiaBitmapImplementation bitmapImpl = new SkiaBitmapImplementation(imgImpl);
            BitmapImplementation = bitmapImpl;
            
            SkiaCanvasImplementation canvasImpl = new SkiaCanvasImplementation(paintImpl, imgImpl, bitmapImpl, pathImpl);
            
            _surfaceImplementation = new SkiaSurfaceImplementation(GraphicsContext, pixmapImpl, canvasImpl, paintImpl);

            canvasImpl.SetSurfaceImplementation(_surfaceImplementation);
            imgImpl.SetSurfaceImplementation(_surfaceImplementation);

            CanvasImplementation = canvasImpl;
        }
        
        public void Setup()
        {
            _surfaceImplementation.GraphicsContext = GraphicsContext;
        }
    }
}
