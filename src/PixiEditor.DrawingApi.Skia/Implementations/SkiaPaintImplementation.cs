using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaPaintImplementation : SkObjectImplementation<SKPaint>, IPaintImplementation
    {
        private readonly SkiaColorFilterImplementation colorFilterImplementation;
        private readonly SkiaImageFilterImplementation imageFilterImplementation;
        private readonly SkiaShaderImplementation shaderImplementation;
 
        public SkiaPaintImplementation(SkiaColorFilterImplementation colorFilterImpl, SkiaImageFilterImplementation imageFilterImpl, SkiaShaderImplementation shaderImpl)
        {
            colorFilterImplementation = colorFilterImpl;
            imageFilterImplementation = imageFilterImpl;
            shaderImplementation = shaderImpl;
        }

        
        public IntPtr CreatePaint()
        {
            SKPaint skPaint = new SKPaint();
            ManagedInstances[skPaint.Handle] = skPaint;
            if (skPaint.ColorFilter != null)
            {
                colorFilterImplementation.ManagedInstances[skPaint.ColorFilter.Handle] = skPaint.ColorFilter;
            }

            return skPaint.Handle;
        }

        public void Dispose(IntPtr paintObjPointer)
        {
            if (!ManagedInstances.TryGetValue(paintObjPointer, out var paint)) return;

            /*if (paint.ColorFilter != null)
            {
                paint.ColorFilter.Dispose();
                colorFilterImplementation.ManagedInstances.TryRemove(paint.ColorFilter.Handle, out _);
            }
            
            if (paint.ImageFilter != null)
            {
                paint.ImageFilter.Dispose();
                imageFilterImplementation.ManagedInstances.TryRemove(paint.ImageFilter.Handle, out _);
            }*/
            
            if (paint.Shader != null)
            {
                paint.Shader.Dispose();
                shaderImplementation.ManagedInstances.TryRemove(paint.Shader.Handle, out _);
            }

            paint.Dispose();
            ManagedInstances.TryRemove(paintObjPointer, out _);
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

        public ColorFilter GetColorFilter(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return new ColorFilter(skPaint.ColorFilter.Handle);
        }

        public void SetColorFilter(Paint paint, ColorFilter? value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.ColorFilter = value == null ? null : colorFilterImplementation[value.ObjectPointer];
        }

        public ImageFilter GetImageFilter(Paint paint)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            return new ImageFilter(skPaint.ColorFilter.Handle);
        }

        public void SetImageFilter(Paint paint, ImageFilter? value)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.ImageFilter = value == null ? null : imageFilterImplementation[value.ObjectPointer];
        }

        public Shader? GetShader(Paint paint)
        {
            if(ManagedInstances.TryGetValue(paint.ObjectPointer, out var skPaint))
            {
                if (skPaint.Shader == null)
                {
                    return null;
                }
                
                return new Shader(skPaint.Shader.Handle);
            }
            
            return null;
        }
        
        public void SetShader(Paint paint, Shader? shader)
        {
            SKPaint skPaint = ManagedInstances[paint.ObjectPointer];
            skPaint.Shader = shader == null ? null : shaderImplementation[shader.ObjectPointer];
        }

        public object GetNativePaint(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer];
        }
    }
}
