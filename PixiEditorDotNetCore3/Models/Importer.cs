using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PixiEditor.Models
{
    public class Importer : NotifyableObject
    { 
        /// <summary>
        /// Imports image from path and resizes it to given dimensions
        /// </summary>
        /// <param name="path">Path of image.</param>
        /// <param name="width">New width of image.</param>
        /// <param name="height">New height of image.</param>
        /// <returns></returns>
        public static WriteableBitmap ImportImage(string path, int width, int height)
        {
            Uri uri = new Uri(path);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.DecodePixelWidth = width;
            bitmap.DecodePixelHeight = height;
            bitmap.EndInit();
            return new WriteableBitmap(bitmap);
        }
    }
}
