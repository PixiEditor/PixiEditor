using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Position;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.DataHolders
{
    public class Surface : IDisposable
    {
        public static SKPaint ReplacingPaint { get; } = new() { BlendMode = SKBlendMode.Src };
        public static SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        public static SKPaint MaskingPaint { get; } = new() { BlendMode = SKBlendMode.DstIn };
        public static SKPaint InverseMaskingPaint { get; } = new() { BlendMode = SKBlendMode.DstOut };

        private static readonly SKPaint nearestNeighborReplacingPaint = new SKPaint() { BlendMode = SKBlendMode.Src, FilterQuality = SKFilterQuality.None };

        public SKSurface SkiaSurface { get; private set; }
        public int Width { get; }
        public int Height { get; }

        public bool Disposed { get; private set; } = false;

        private SKPaint drawingPaint = new SKPaint() { BlendMode = SKBlendMode.Src };
        private IntPtr surfaceBuffer;

        public Surface(int w, int h)
        {
            if (w <= 0 || h <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");
            InitSurface(w, h);
            Width = w;
            Height = h;
        }

        public Surface(Surface original)
        {
            Width = original.Width;
            Height = original.Height;
            InitSurface(Width, Height);
            original.SkiaSurface.Draw(SkiaSurface.Canvas, 0, 0, ReplacingPaint);
        }

        public Surface(int w, int h, byte[] pbgra32Bytes)
        {
            if (w <= 0 || h <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");
            Width = w;
            Height = h;
            InitSurface(w, h);
            DrawBytes(w, h, pbgra32Bytes, SKColorType.Bgra8888, SKAlphaType.Premul);
        }

        public Surface(BitmapSource original)
        {
            SKColorType color = original.Format.ToSkia(out SKAlphaType alpha);
            if (original.PixelWidth <= 0 || original.PixelHeight <= 0)
                throw new ArgumentException("Surface dimensions must be non-zero");

            int stride = (original.PixelWidth * original.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[stride * original.PixelHeight];
            original.CopyPixels(pixels, stride, 0);

            Width = original.PixelWidth;
            Height = original.PixelHeight;
            InitSurface(Width, Height);
            DrawBytes(Width, Height, pixels, color, alpha);
        }

        public Surface(SKImage image)
        {
            Width = image.Width;
            Height = image.Height;
            InitSurface(Width, Height);
            SkiaSurface.Canvas.DrawImage(image, 0, 0);
        }

        /// <summary>
        /// Combines the <paramref name="images"/> into a <see cref="Surface"/>
        /// </summary>
        /// <param name="width">The width of the <see cref="Surface"/></param>
        /// <param name="height">The height of the <see cref="Surface"/></param>
        /// <returns>A surface that has the <paramref name="images"/> drawn on it</returns>
        public static Surface Combine(int width, int height, IEnumerable<(SKImage image, Coordinates offset)> images)
        {
            Surface surface = new Surface(width, height);

            foreach (var image in images)
            {
                surface.SkiaSurface.Canvas.DrawImage(image.image, (SKPoint)image.offset);
            }

            return surface;
        }

        public Surface ResizeNearestNeighbor(int newW, int newH)
        {
            SKImage image = SkiaSurface.Snapshot();
            Surface newSurface = new(newW, newH);
            newSurface.SkiaSurface.Canvas.DrawImage(image, new SKRect(0, 0, newW, newH), nearestNeighborReplacingPaint);
            return newSurface;
        }

        public Surface Crop(int x, int y, int width, int height)
        {
            Surface result = new Surface(width, height);
            SkiaSurface.Draw(result.SkiaSurface.Canvas, x, y, ReplacingPaint);
            return result;
        }

        public unsafe SKColor GetSRGBPixel(int x, int y)
        {
            Half* ptr = (Half*)(surfaceBuffer + (x + y * Width) * 8);
            float a = (float)ptr[3];
            return (SKColor)new SKColorF((float)ptr[0] / a, (float)ptr[1] / a, (float)ptr[2] / a, (float)ptr[3]);
        }

        public void SetSRGBPixel(int x, int y, SKColor color)
        {
            // It's possible that this function can be sped up by writing into surfaceBuffer, not sure if skia will like it though
            drawingPaint.Color = color;
            SkiaSurface.Canvas.DrawPoint(x, y, drawingPaint);
        }

        public unsafe void SetSRGBPixelUnmanaged(int x, int y, SKColor color)
        {
            Half* ptr = (Half*)(surfaceBuffer + (x + y * Width) * 8);

            float normalizedAlpha = color.Alpha / 255.0f;

            ptr[0] = (Half)(color.Red / 255f * normalizedAlpha);
            ptr[1] = (Half)(color.Green / 255f * normalizedAlpha);
            ptr[2] = (Half)(color.Blue / 255f * normalizedAlpha);
            ptr[3] = (Half)(normalizedAlpha);
        }

        public unsafe byte[] ToByteArray(SKColorType colorType = SKColorType.Bgra8888, SKAlphaType alphaType = SKAlphaType.Premul)
        {
            var imageInfo = new SKImageInfo(Width, Height, colorType, alphaType, SKColorSpace.CreateSrgb());

            byte[] buffer = new byte[Width * Height * imageInfo.BytesPerPixel];
            fixed (void* pointer = buffer)
            {
                if (!SkiaSurface.ReadPixels(imageInfo, new IntPtr(pointer), imageInfo.RowBytes, 0, 0))
                {
                    throw new InvalidOperationException("Could not read surface into buffer");
                }
            }

            return buffer;
        }

        public WriteableBitmap ToWriteableBitmap()
        {
            WriteableBitmap result = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32, null);
            result.Lock();
            var dirty = new Int32Rect(0, 0, Width, Height);
            result.WritePixels(dirty, ToByteArray(), Width * 4, 0);
            result.AddDirtyRect(dirty);
            result.Unlock();
            return result;
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            SkiaSurface.Dispose();
            drawingPaint.Dispose();
            Marshal.FreeHGlobal(surfaceBuffer);
            GC.SuppressFinalize(this);
        }

        ~Surface()
        {
            Marshal.FreeHGlobal(surfaceBuffer);
        }

        private static SKSurface CreateSurface(int w, int h, IntPtr buffer)
        {
            var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb()), buffer);
            if (surface == null)
                throw new Exception("Could not create surface");
            return surface;
        }
        private static SKSurface CreateSurface(int w, int h)
        {
            var surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.RgbaF16, SKAlphaType.Premul, SKColorSpace.CreateSrgb()));
            if (surface == null)
                throw new Exception("Could not create surface");
            return surface;
        }

        private unsafe void InitSurface(int w, int h)
        {
            int byteC = w * h * 8;
            surfaceBuffer = Marshal.AllocHGlobal(byteC);
            Unsafe.InitBlockUnaligned((byte*)surfaceBuffer, 0, (uint)byteC);
            SkiaSurface = CreateSurface(w, h, surfaceBuffer);
        }

        private unsafe void DrawBytes(int w, int h, byte[] bytes, SKColorType colorType, SKAlphaType alphaType)
        {
            SKImageInfo info = new SKImageInfo(w, h, colorType, alphaType);

            fixed (void* pointer = bytes)
            {
                using SKPixmap map = new(info, new IntPtr(pointer));
                using SKSurface surface = SKSurface.Create(map);
                surface.Draw(SkiaSurface.Canvas, 0, 0, ReplacingPaint);
            }
        }

#if DEBUG
        // Used to iterate the surface's pixels during development

        [Obsolete("Only meant for use in a debugger like Visual Studio", true)]
        private SurfaceDebugger Debugger => new SurfaceDebugger(this);

        [Obsolete("Only meant for use in a debugger like Visual Studio", true)]
        private class SurfaceDebugger : IEnumerable
        {
            private readonly Surface _surface;

            public SurfaceDebugger(Surface surface)
            {
                _surface = surface;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                var pixmap = _surface.SkiaSurface.PeekPixels();

                for (int y = 0; y < pixmap.Width; y++)
                {
                    yield return new DebugPixel(y);

                    for (int x = 0; x < pixmap.Height; x++)
                    {
                        yield return new DebugPixel(x, y, pixmap.GetPixelColor(x, y).ToString());
                    }
                }
            }

            [DebuggerDisplay("{DebuggerDisplay,nq}")]
            private struct DebugPixel
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private string DebuggerDisplay
                {
                    get
                    {
                        if (isPixel)
                        {
                            return $"X: {x}; Y: {y} - {hex}";
                        }
                        else
                        {
                            return $"|- Y: {y} -|";
                        }
                    }
                }

                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly int x;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly int y;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly string hex;
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly bool isPixel;

                public DebugPixel(int y)
                {
                    x = 0;
                    this.y = y;
                    hex = null;
                    isPixel = false;
                }

                public DebugPixel(int x, int y, string hex)
                {
                    this.x = x;
                    this.y = y;
                    this.hex = hex;
                    isPixel = true;
                }
            }
        }
#endif
    }
}
