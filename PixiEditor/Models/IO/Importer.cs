using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Parser;

namespace PixiEditor.Models.IO
{
    public class Importer : NotifyableObject
    {
        /// <summary>
        ///     Imports image from path and resizes it to given dimensions.
        /// </summary>
        /// <param name="path">Path of image.</param>
        /// <param name="width">New width of image.</param>
        /// <param name="height">New height of image.</param>
        /// <returns>WriteableBitmap of imported image.</returns>
        public static WriteableBitmap ImportImage(string path, int width, int height)
        {
            WriteableBitmap wbmp = ImportImage(path);
            if (wbmp.PixelWidth != width || wbmp.PixelHeight != height)
            {
                return wbmp.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            }

            return wbmp;
        }

        /// <summary>
        ///     Imports image from path and resizes it to given dimensions.
        /// </summary>
        /// <param name="path">Path of image.</param>
        public static WriteableBitmap ImportImage(string path)
        {
            try
            {
                Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                return BitmapFactory.ConvertToPbgra32Format(bitmap);
            }
            catch (NotSupportedException)
            {
                throw new CorruptedFileException();
            }
            catch (FileFormatException)
            {
                throw new CorruptedFileException();
            }
        }

        public static Document ImportDocument(string path)
        {
            try
            {
                Document doc = PixiEditor.Parser.PixiParser.Deserialize(path).ToDocument();
                doc.DocumentFilePath = path;
                return doc;
            }
            catch (InvalidFileException)
            {
                throw new CorruptedFileException();
            }
        }

        public static bool IsSupportedFile(string path)
        {
            path = path.ToLower();
            return path.EndsWith(".pixi") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg");
        }
    }
}