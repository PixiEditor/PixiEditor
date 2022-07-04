using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using SkiaSharp;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views.UserControls
{
    public partial class PlainLayerView : UserControl
    {
        public static readonly DependencyProperty TargetLayerProperty =
            DependencyProperty.Register(nameof(TargetLayer), typeof(Layer), typeof(PlainLayerView), new PropertyMetadata(null, OnLayerChanged));

        public Layer TargetLayer
        {
            get => (Layer)GetValue(TargetLayerProperty);
            set => SetValue(TargetLayerProperty, value);
        }

        private SurfaceRenderer renderer;
        private int prevLayerWidth = -1;
        private int prevLayerHeight = -1;

        private Int32Rect _cachedTightBounds;

        public PlainLayerView()
        {
            InitializeComponent();
            SizeChanged += OnControlSizeChanged;
        }

        public void Resize(int newWidth, int newHeight)
        {
            renderer?.Dispose();
            renderer = new SurfaceRenderer(newWidth, newHeight);
            image.Source = renderer.FinalBitmap;
            Update();
        }

        private static void OnLayerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var view = (PlainLayerView)sender;
            if (args.OldValue != null)
                ((Layer)args.OldValue).LayerBitmapChanged -= view.OnLayerBitmapChanged;

            if (args.NewValue != null)
            {
                var layer = ((Layer)args.NewValue);
                if (layer.LayerBitmap.Disposed)
                {
                    view.TargetLayer = null;
                    return;
                }

                layer.LayerBitmapChanged += view.OnLayerBitmapChanged;
                view._cachedTightBounds = GetTightBounds(layer);

                view.Resize(view._cachedTightBounds.Width, view._cachedTightBounds.Height);
            }
        }

        private void Update()
        {
            renderer.Draw(TargetLayer.LayerBitmap, (byte)(TargetLayer.Opacity * 255), SKRectI.Create(_cachedTightBounds.X, _cachedTightBounds.Y, _cachedTightBounds.Width, _cachedTightBounds.Height));
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TargetLayer == null)
                return;
            ResizeWithOptimized(e.NewSize);
        }

        private void ResizeWithOptimized(Size newSize)
        {
            var (w, h) = GetOptimizedDimensions(TargetLayer.Width, TargetLayer.Height, newSize.Width, newSize.Height);
            Resize(w, h);
        }

        private (int, int) GetOptimizedDimensions(int width, int height, double viewWidth, double viewHeight)
        {
            if (width <= viewWidth && height <= viewHeight)
                return (width, height);

            double frac = width / (double)height;
            double viewFrac = viewWidth / viewHeight;

            if (frac > viewFrac)
            {
                double targetWidth = viewWidth;
                double targetHeight = viewWidth / frac;
                return ((int)Math.Ceiling(targetWidth), (int)Math.Ceiling(targetHeight));
            }
            else
            {
                double targetHeight = viewHeight;
                double targetWidth = targetHeight * frac;
                return ((int)Math.Ceiling(targetWidth), (int)Math.Ceiling(targetHeight));
            }
        }

        private void OnLayerBitmapChanged(object sender, Int32Rect e)
        {
            if (TargetLayer.Width != prevLayerWidth || TargetLayer.Height != prevLayerHeight 
                || TargetLayer.OffsetX != _cachedTightBounds.X
                || TargetLayer.OffsetY != _cachedTightBounds.Y)
            {
                ResizeWithOptimized(RenderSize);
                prevLayerWidth = TargetLayer.Width;
                prevLayerHeight = TargetLayer.Height;
                _cachedTightBounds = GetTightBounds(TargetLayer);
            }
            else
            {
                Update();
            }
        }

        private static Int32Rect GetTightBounds(Layer targetLayer)
        {
            //var tightBounds = targetLayer.TightBounds;
            //if (tightBounds.IsEmpty)
            //{
            //    tightBounds = new Int32Rect(0, 0, targetLayer.Width, targetLayer.Height);
            //}

            var tightBounds = new Int32Rect(0, 0, targetLayer.Width, targetLayer.Height);

            return tightBounds;
        }
    }
}
