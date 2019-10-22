using PixiEditorDotNetCore3.Models.Tools;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Layers
{
    public class Layer : BasicLayer
    {
        private WriteableBitmap _layerBitmap;
        public string Name { get; set; }
        public bool IsVisible { get; set; } = true;

        public WriteableBitmap LayerBitmap
        {
            get { return _layerBitmap; }
            set
            {
                _layerBitmap = value;
                RaisePropertyChanged("LayerBitmap");
            }
        }

        public Layer(string name,int width, int height)
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

        public void ApplyPixels(BitmapPixelChanges pixels, Color color)
        {
            LayerBitmap.Lock();

            foreach (var coords in pixels.ChangedCoordinates)
            {
                LayerBitmap.SetPixel(Math.Clamp(coords.X, 0, Width - 1), Math.Clamp(coords.Y, 0, Height - 1),
                    color);
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
