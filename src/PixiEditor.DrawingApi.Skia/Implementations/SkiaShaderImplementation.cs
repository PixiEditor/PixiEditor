using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaShaderImplementation : SkObjectImplementation<SKShader>, IShaderImplementation
    {
        public SkiaShaderImplementation()
        {
        }

        public IntPtr CreateShader()
        {
            SKShader skShader = SKShader.CreateEmpty();
            ManagedInstances[skShader.Handle] = skShader;
            return skShader.Handle;
        }

        public Shader? CreateFromSksl(string sksl, bool isOpaque, out string errors)
        {
            SKRuntimeEffect effect = SKRuntimeEffect.Create(sksl, out errors);
            
            if (string.IsNullOrEmpty(errors))
            {
                SKShader shader = effect.ToShader(isOpaque);
                ManagedInstances[shader.Handle] = shader;
                return new Shader(shader.Handle);
            }
            
            return null;
        }
        
        public Shader CreateLinearGradient(VecI p1, VecI p2, Color[] colors)
        {
            SKShader shader = SKShader.CreateLinearGradient(
                new SKPoint(p1.X, p1.Y), 
                new SKPoint(p2.X, p2.Y),
                CastUtility.UnsafeArrayCast<Color, SKColor>(colors),
                null, 
                SKShaderTileMode.Clamp);
            ManagedInstances[shader.Handle] = shader;
            return new Shader(shader.Handle);
        }

        public object GetNativeShader(IntPtr objectPointer)
        {
            return ManagedInstances[objectPointer]; 
        }

        public void Dispose(IntPtr shaderObjPointer)
        {
            if (!ManagedInstances.TryGetValue(shaderObjPointer, out var shader)) return;
            shader.Dispose();
            ManagedInstances.TryRemove(shaderObjPointer, out _);
        }
    }
}
