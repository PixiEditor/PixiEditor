using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class LayerStackRenderer : INotifyPropertyChanged, IDisposable
    {
        private SKPaint BlendingPaint { get; } = new SKPaint() { BlendMode = SKBlendMode.SrcOver };

        private ObservableCollection<Layer> layers;
        private LayerStructure structure;

        private Surface finalSurface;
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

        public Surface FinalSurface { get => finalSurface; }

        public event PropertyChangedEventHandler PropertyChanged;
        public LayerStackRenderer(ObservableCollection<Layer> layers, LayerStructure structure, int width, int height)
        {
            this.layers = layers;
            this.structure = structure;
            layers.CollectionChanged += OnLayersChanged;
            Resize(width, height);
        }

        public void Resize(int newWidth, int newHeight)
        {
            finalSurface?.Dispose();
            backingSurface?.Dispose();
            finalSurface = new Surface(newWidth, newHeight);
            FinalBitmap = new WriteableBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32, null);
            var imageInfo = new SKImageInfo(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            backingSurface = SKSurface.Create(imageInfo, finalBitmap.BackBuffer, finalBitmap.BackBufferStride);
            Update(new Int32Rect(0, 0, newWidth, newHeight));
        }

        public void SetNewLayersCollection(ObservableCollection<Layer> layers)
        {
            layers.CollectionChanged -= OnLayersChanged;
            this.layers = layers;
            layers.CollectionChanged += OnLayersChanged;
            Update(new Int32Rect(0, 0, finalSurface.Width, finalSurface.Height));
        }

        public void Dispose()
        {
            finalSurface.Dispose();
            backingSurface.Dispose();
            BlendingPaint.Dispose();
            layers.CollectionChanged -= OnLayersChanged;
        }

        private void Update(Int32Rect dirtyRectangle)
        {
            finalSurface.SkiaSurface.Canvas.Clear();
            foreach (var layer in layers)
            {
                if (!layer.IsVisible)
                    continue;
                BlendingPaint.Color = new SKColor(255, 255, 255, (byte)(LayerStructureUtils.GetFinalLayerOpacity(layer, structure) * 255));
                layer.LayerBitmap.SkiaSurface.Draw(
                    finalSurface.SkiaSurface.Canvas,
                    layer.OffsetX,
                    layer.OffsetY,
                    BlendingPaint);
            }
            finalBitmap.Lock();
            finalSurface.SkiaSurface.Draw(backingSurface.Canvas, 0, 0, Surface.ReplacingPaint);
            finalBitmap.AddDirtyRect(dirtyRectangle.Min(new Int32Rect(0, 0, finalBitmap.PixelWidth, finalBitmap.PixelHeight)));
            finalBitmap.Unlock();
        }

        private void OnLayersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var obj in e.NewItems)
                {
                    Layer layer = (Layer)obj;
                    layer.LayerBitmapChanged += OnLayerBitmapChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var obj in e.OldItems)
                {
                    ((Layer)obj).LayerBitmapChanged -= OnLayerBitmapChanged;
                }
            }
            Update(new Int32Rect(0, 0, finalSurface.Width, finalSurface.Height));
        }

        private void OnLayerBitmapChanged(object sender, Int32Rect e)
        {
            Update(e);
        }
    }
}
