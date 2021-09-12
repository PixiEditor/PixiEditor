using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
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

        public PlainLayerView()
        {
            InitializeComponent();
            SizeChanged += OnControlSizeChanged;
            Unloaded += OnControlUnloaded;
        }

        private static void OnLayerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var view = (PlainLayerView)sender;
            if (args.OldValue != null)
                ((Layer)args.OldValue).LayerBitmapChanged -= view.OnLayerBitmapChanged;
            if (args.NewValue != null)
            {
                var layer = ((Layer)args.NewValue);
                layer.LayerBitmapChanged += view.OnLayerBitmapChanged;
                view.Resize(layer.Width, layer.Height);
            }
        }
        public void Resize(int newWidth, int newHeight)
        {
            renderer?.Dispose();
            renderer = new SurfaceRenderer(newWidth, newHeight);
            image.Source = renderer.FinalBitmap;
            Update();
        }

        private void Update()
        {
            renderer.Draw(TargetLayer.LayerBitmap, (byte)(TargetLayer.Opacity * 255));
        }

        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            /*
            if (LogicalTreeHelper.GetParent(this) != null)
                return;
            renderer?.Dispose();
            TargetLayer.LayerBitmapChanged -= OnLayerBitmapChanged;*/
        }

        private void OnControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (TargetLayer == null)
                return;
            MaybeResize(e.NewSize);
        }

        private bool MaybeResize(Size newSize)
        {
            var (w, h) = GetOptimizedDimensions(TargetLayer.Width, TargetLayer.Height, newSize.Width, newSize.Height);
            Resize(w, h);
            return true;
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
            if (!MaybeResize(RenderSize))
                Update();
        }
    }
}
