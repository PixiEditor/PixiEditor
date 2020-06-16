using System;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Layers
{
    public class Layer : BasicLayer
    {
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool IsRenaming
        {
            get => _isRenaming;
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

        private bool _isActive;

        private bool _isRenaming;
        private bool _isVisible = true;
        private WriteableBitmap _layerBitmap;

        private string _name;

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
            Width = (int) layerBitmap.Width;
            Height = (int) layerBitmap.Height;
        }

        /// <summary>
        ///     Applies pixels to layer
        /// </summary>
        /// <param name="pixels"></param>
        public void ApplyPixels(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels == null) return;
            using (var ctx = LayerBitmap.GetBitmapContext())
            {
                foreach (var coords in pixels.ChangedPixels)
                {
                    if (coords.Key.X > Width - 1 || coords.Key.X < 0 || coords.Key.Y < 0 ||
                        coords.Key.Y > Height - 1) continue;
                    ctx.WriteableBitmap.SetPixel(Math.Clamp(coords.Key.X, 0, Width - 1),
                        Math.Clamp(coords.Key.Y, 0, Height - 1),
                        coords.Value);
                }
            }
        }

        public void Clear()
        {
            LayerBitmap.Clear();
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