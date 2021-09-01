using PixiEditor.Models.Layers;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class SingleLayerRenderer : INotifyPropertyChanged, IDisposable
    {
        private SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };
        private Layer layer;

        private SKSurface backingSurface;
        private WriteableBitmap finalBitmap;
        public WriteableBitmap FinalBitmap
        {
            get => finalBitmap;
            set
            {
                finalBitmap = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FinalBitmap)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public SingleLayerRenderer(Layer layer, int width, int height)
        {
            this.layer = layer;
            layer.LayerBitmapChanged += OnLayerBitmapChanged;
            Resize(width, height);
        }

        public void Resize(int newWidth, int newHeight)
        {
            backingSurface?.Dispose();

            finalBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32, null);
            var imageInfo = new SKImageInfo(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            backingSurface = SKSurface.Create(imageInfo, finalBitmap.BackBuffer, finalBitmap.BackBufferStride);
            Update(new Int32Rect(0, 0, newWidth, newHeight));
        }

        public void Dispose()
        {
            backingSurface.Dispose();
            BlendingPaint.Dispose();
            layer.LayerBitmapChanged -= OnLayerBitmapChanged;
        }

        private void Update(Int32Rect dirtyRectangle)
        {
            backingSurface.Canvas.Clear();
            finalBitmap.Lock();
            if (layer.IsVisible)
            {
                BlendingPaint.Color = new SKColor(255, 255, 255, (byte)(layer.Opacity * 255));
                layer.LayerBitmap.SkiaSurface.Draw(
                    backingSurface.Canvas,
                    layer.OffsetX,
                    layer.OffsetY,
                    BlendingPaint);
            }
            finalBitmap.AddDirtyRect(dirtyRectangle);
            finalBitmap.Unlock();
        }

        private void OnLayerBitmapChanged(object sender, Int32Rect e)
        {
            Update(e);
        }
    }
}
