using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Surfaces.PaintImpl
{
    /// <summary>
    ///     Class used to define surface paint, which is a collection of paint operations.
    /// </summary>
    public class Paint : NativeObject
    {
        private ImageFilter? imageFilter;
        private ColorFilter? colorFilter;
        private Shader? shader;
        
        public override object Native => DrawingBackendApi.Current.PaintImplementation.GetNativePaint(ObjectPointer);

        public Color Color
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetColor(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetColor(this, value);
        }

        public BlendMode BlendMode
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetBlendMode(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetBlendMode(this, value);
        }
        
        public FilterQuality FilterQuality 
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetFilterQuality(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetFilterQuality(this, value);
        }
        
        public bool IsAntiAliased 
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetIsAntiAliased(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetIsAntiAliased(this, value);
        }
        
        public PaintStyle Style 
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStyle(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStyle(this, value);
        }
        
        public StrokeCap StrokeCap 
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeCap(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeCap(this, value);
        }
        
        public float StrokeWidth 
        {
            get => DrawingBackendApi.Current.PaintImplementation.GetStrokeWidth(this);
            set => DrawingBackendApi.Current.PaintImplementation.SetStrokeWidth(this, value);
        }
        
        public ColorFilter ColorFilter
        {
            get => colorFilter ??= DrawingBackendApi.Current.PaintImplementation.GetColorFilter(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetColorFilter(this, value);
                colorFilter = value;
            }
        }

        public ImageFilter ImageFilter
        {
            get => imageFilter ??= DrawingBackendApi.Current.PaintImplementation.GetImageFilter(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetImageFilter(this, value);
                imageFilter = value;
            }
        }

        public Shader Shader
        {
            get => shader ??= DrawingBackendApi.Current.PaintImplementation.GetShader(this);
            set
            {
                DrawingBackendApi.Current.PaintImplementation.SetShader(this, value);
                shader = value;
            }
        }

        public Paint(IntPtr objPtr) : base(objPtr)
        {
        }
        
        public Paint() : base(DrawingBackendApi.Current.PaintImplementation.CreatePaint())
        {
        }
        
        public Paint Clone()
        {
            return DrawingBackendApi.Current.PaintImplementation.Clone(ObjectPointer);
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.PaintImplementation.Dispose(ObjectPointer);
        }
    }
}
