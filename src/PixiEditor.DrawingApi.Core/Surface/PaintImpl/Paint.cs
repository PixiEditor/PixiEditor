using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Surface
{
    /// <summary>
    ///     Class used to define surface paint, which is a collection of paint operations.
    /// </summary>
    public class Paint : NativeObject
    {
        public Color Color { get; set; }
        public BlendMode BlendMode { get; set; } = BlendMode.Src;
        public FilterQuality FilterQuality { get; set; } = FilterQuality.None;
        public bool IsAntiAliased { get; set; } = false;
        public PaintStyle Style { get; set; }
        public StrokeCap StrokeCap { get; set; }
        public float StrokeWidth { get; set; }
        
        
        internal Paint(IntPtr objPtr) : base(objPtr)
        {
        }
        
        public Paint() : base(DrawingBackendApi.Current.PaintImplementation.CreatePaint())
        {
        }

        public override void Dispose()
        {
            DrawingBackendApi.Current.PaintImplementation.Dispose(ObjectPointer);
        }
    }
}
