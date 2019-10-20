using PixiEditorDotNetCore3.Models.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorDotNetCore3.Models.Layers
{
    public static class LayerGenerator
    {
        /// <summary>
        /// Generating useable layer with image and bitmap
        /// </summary>
        /// <param name="imageWidth">Width of layer.</param>
        /// <param name="imageHeight">Height of layer.</param>
        /// <returns></returns>
        public static Layer Generate(int imageWidth, int imageHeight)
        {
            return new Layer(GenerateBitmap(imageWidth, imageHeight));
        }

        public static LightLayer GenerateWithByteArray(int width, int height)
        {
            WriteableBitmap bitmap = GenerateBitmap(width, height);
            bitmap.Lock();
            byte[] byteArray = bitmap.ToByteArray();
            bitmap.Unlock();
            return new LightLayer(byteArray, height, width);
        }

        /// <summary>
        /// Generates bitmap ready to work with
        /// </summary>
        /// <param name="bitmapWidth">Width of bitmap.</param>
        /// <param name="imageHeight">Height of bitmap.</param>
        /// <returns></returns>
        private static WriteableBitmap GenerateBitmap(int bitmapWidth, int imageHeight)
        {
            WriteableBitmap bitmap = BitmapFactory.New(bitmapWidth, imageHeight);
            bitmap.Clear(System.Windows.Media.Colors.Transparent);
            return bitmap;
        }

    }
}
