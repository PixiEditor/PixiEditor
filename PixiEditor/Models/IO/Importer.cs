using System;
using Avalonia.Media.Imaging;
using AvaloniaWriteableBitmapEx;
using PixiEditor.Models.DataHolders;
using ReactiveUI;

namespace PixiEditor.Models.IO
{
    public class Importer : ReactiveObject
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
            if (wbmp.PixelSize.Width != width || wbmp.PixelSize.Height != height)
            {
                return wbmp.Resize(width, height, WriteableBitmapEx.Interpolation.NearestNeighbor);
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

            return /*BitmapFactory.ConvertToPbgra32Format(*/bitmap;//); TODO: Implement this
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