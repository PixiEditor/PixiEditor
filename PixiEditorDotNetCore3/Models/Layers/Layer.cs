using PixiEditor.Models.Tools;
using System;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Layers
{
    public class Layer : BasicLayer
    {
        private WriteableBitmap _layerBitmap;
        public string Name { get; set; }
        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public WriteableBitmap LayerBitmap
        {
            get => _layerBitmap;
            set
            {
                _layerBitmap = value;
                RaisePropertyChanged("LayerBitmap");
            }
        }

        public Layer(string name, int width, int height)
        {
            Name = name;
            Layer layer = LayerGenerator.Generate(width, height);
            LayerBitmap = layer.LayerBitmap;
            Width = width;
            Height = height;
        }


        public Layer(WriteableBitmap layerBitmap)
        {
            LayerBitmap = layerBitmap;
            Width = (int)layerBitmap.Width;
            Height = (int)layerBitmap.Height;
        }

        public void ApplyPixels(BitmapPixelChanges pixels)
        {
            LayerBitmap.Lock();

            foreach (var coords in pixels.ChangedPixels)
            {
                LayerBitmap.SetPixel(Math.Clamp(coords.Key.X, 0, Width - 1), Math.Clamp(coords.Key.Y, 0, Height - 1),
                    coords.Value);
            }

            LayerBitmap.Unlock();
        }

        public byte[] ConvertBitmapToBytes()
        {            
            LayerBitmap.Lock();
            byte[] byteArray = LayerBitmap.ToByteArray();
            LayerBitmap.Unlock();
            return byteArray;
        }

        public byte[] ConvertBitmapToBytes(WriteableBitmap bitmap)
        {
            bitmap.Lock();
            byte[] byteArray = bitmap.ToByteArray();
            bitmap.Unlock();
            return byteArray;
        }

        public void Resize(int newWidth, int newHeight)
        {
            LayerBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            Height = newHeight;
            Width = newWidth;
        }

    }
}
