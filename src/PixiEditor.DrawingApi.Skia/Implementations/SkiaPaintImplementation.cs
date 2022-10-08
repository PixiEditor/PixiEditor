using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaPaintImplementation : SkObjectImplementation<SKPaint>, IPaintImplementation
    {
        public IntPtr CreatePaint()
        {
            SKPaint skPaint = new SKPaint();
            skPaint.IsAntialias = false;
            skPaint.BlendMode = SKBlendMode.Src;
            skPaint.FilterQuality = SKFilterQuality.None;
            
            ManagedInstances[skPaint.Handle] = skPaint;
            return skPaint.Handle;
        }

        public void Dispose(IntPtr paintObjPointer)
        {
            if (!ManagedInstances.ContainsKey(paintObjPointer)) return;
            SKPaint paint = ManagedInstances[paintObjPointer];
            paint.Dispose();
            ManagedInstances.Remove(paintObjPointer);
        }

        public Paint Clone(IntPtr paintObjPointer)
        {
            SKPaint clone = ManagedInstances[paintObjPointer].Clone();
            ManagedInstances[clone.Handle] = clone;
            return new Paint(clone.Handle);
        }

        public Color GetColor(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return skPaint.Color.ToBackendColor();
        }

        public void SetColor(Paint paint, Color value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.Color = value.ToSKColor();
        }

        public BlendMode GetBlendMode(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return (BlendMode)skPaint.BlendMode;
        }

        public void SetBlendMode(Paint paint, BlendMode value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.BlendMode = (SKBlendMode)value;
        }

        public FilterQuality GetFilterQuality(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return (FilterQuality)skPaint.FilterQuality;
        }

        public void SetFilterQuality(Paint paint, FilterQuality value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.FilterQuality = (SKFilterQuality)value;
        }

        public bool GetIsAntiAliased(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return skPaint.IsAntialias;
        }

        public void SetIsAntiAliased(Paint paint, bool value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.IsAntialias = value;
        }

        public PaintStyle GetStyle(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return (PaintStyle)skPaint.Style;
        }

        public void SetStyle(Paint paint, PaintStyle value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.Style = (SKPaintStyle)value;
        }

        public StrokeCap GetStrokeCap(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return (StrokeCap)skPaint.StrokeCap;
        }

        public void SetStrokeCap(Paint paint, StrokeCap value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.StrokeCap = (SKStrokeCap)value;
        }

        public float GetStrokeWidth(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return skPaint.StrokeWidth;
        }

        public void SetStrokeWidth(Paint paint, float value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.StrokeWidth = value;
        }
    }
}
