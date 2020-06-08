using PixiEditor.Models.Tools;
using System;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Layers
{
    public class Layer : BasicLayer
    {
        private WriteableBitmap _layerBitmap;
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }


        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }
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

        private bool _isRenaming = false;

        public bool IsRenaming
        {
            get { return _isRenaming; }
            set
            {
                _isRenaming = value;
                RaisePropertyChanged("IsRenaming");
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

        /// <summary>
        /// Applies pixels to layer
        /// </summary>
        /// <param name="pixels"></param>
        public void ApplyPixels(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels == null) return;
            using (LayerBitmap.GetBitmapContext())
            {

                foreach (var coords in pixels.ChangedPixels)
                {
                    if (coords.Key.X > Width - 1 || coords.Key.X < 0 || coords.Key.Y < 0 || coords.Key.Y > Height - 1) continue;
                    LayerBitmap.SetPixel(Math.Clamp(coords.Key.X, 0, Width - 1), Math.Clamp(coords.Key.Y, 0, Height - 1),
                        coords.Value);
                }
            }
        }

        public void Clear()
        {
            LayerBitmap.Lock();
            LayerBitmap.Clear();
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
