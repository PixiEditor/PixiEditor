using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models
{
    public class Layer : NotifyableObject
    {
        private WriteableBitmap _layerBitmap;

        public WriteableBitmap LayerBitmap
        {
            get { return _layerBitmap; }
            set {
                _layerBitmap = value;
                RaisePropertyChanged("LayerBitmap");
            }
        }

        private Image _layerImage;

        public Image LayerImage
        {
            get { return _layerImage; }
            set {
                _layerImage = value;
                RaisePropertyChanged("LayerImage");
            }
        }

        private int _width;

        public int Width
        {
            get { return _width; }
            set { _width = value; RaisePropertyChanged("Width"); }
        }

        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; RaisePropertyChanged("Height"); }
        }



        public Layer(int width, int height)
        {
            Layer layer = LayerGenerator.GenerateLayer(width, height);
            LayerBitmap = layer.LayerBitmap;
            LayerImage = layer.LayerImage;
            Width = width;
            Height = height;
        }


        public Layer(WriteableBitmap layerBitmap, Image layerImage)
        {
            LayerBitmap = layerBitmap;
            LayerImage = layerImage;
            Width = (int)layerImage.Width;
            Height = (int)layerImage.Height;
        }
    }
}
