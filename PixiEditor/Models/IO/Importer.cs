using System;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.IO
{
    public class Importer : NotifyableObject
    {
        /// <summary>
        ///     Imports image from path and resizes it to given dimensions
        /// </summary>
        /// <param name="path">Path of image.</param>
        /// <param name="width">New width of image.</param>
        /// <param name="height">New height of image.</param>
        /// <returns></returns>
        public static WriteableBitmap ImportImage(string path, int width, int height)
        {
            var wbmp = ImportImage(path);
            if (wbmp.PixelWidth != width || wbmp.PixelHeight != height)
            {
                return wbmp.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }

            return wbmp;
        }

        /// <summary>
        ///     Imports image from path and resizes it to given dimensions
        /// </summary>
        /// <param name="path">Path of image.</param>
        public static WriteableBitmap ImportImage(string path)
        {
            Uri uri = new Uri(path);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.EndInit();

            return BitmapFactory.ConvertToPbgra32Format(bitmap);
        }

        public static Document ImportDocument(string path)
        {
            return BinarySerialization.ReadFromBinaryFile<SerializableDocument>(path).ToDocument();
        }

        public static bool IsSupportedFile(string path)
        {
            path = path.ToLower();
            return path.EndsWith(".pixi") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg");
        }
    }
}