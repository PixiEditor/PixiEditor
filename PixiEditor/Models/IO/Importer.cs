using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Parser;
using SkiaSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

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
        public static Surface ImportImage(string path, int width, int height)
        {
            Surface wbmp = ImportSurface(path);
            if (wbmp.Width != width || wbmp.Height != height)
            {
                var resized = wbmp.ResizeNearestNeighbor(width, height);
                wbmp.Dispose();
                return resized;
            }

            return wbmp;
        }

        /// <summary>
        ///     Imports image from path and resizes it to given dimensions.
        /// </summary>
        /// <param name="path">Path of image.</param>
        public static Surface ImportSurface(string path)
        {
            try
            {
                using var image = SKImage.FromEncodedData(path);
                Surface surface = new Surface(image.Width, image.Height);
                surface.SkiaSurface.Canvas.DrawImage(image, new SKPoint(0, 0));
                return surface;
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

        public static WriteableBitmap ImportWriteableBitmap(string path)
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

        public static Surface LoadFromGZippedBytes(string path)
        {
            using FileStream compressedData = new(path, FileMode.Open);
            using GZipStream uncompressedData = new(compressedData, CompressionMode.Decompress);
            using MemoryStream resultBytes = new();
            uncompressedData.CopyTo(resultBytes);

            byte[] bytes = resultBytes.ToArray();
            int width = BitConverter.ToInt32(bytes, 0);
            int height = BitConverter.ToInt32(bytes, 4);

            SKImageInfo info = new SKImageInfo(width, height, SKColorType.RgbaF16);
            var ptr = Marshal.AllocHGlobal(bytes.Length - 8);
            try
            {
                Marshal.Copy(bytes, 8, ptr, bytes.Length - 8);
                SKPixmap map = new(info, ptr);
                SKSurface surface = SKSurface.Create(map);
                var finalSurface = new Surface(width, height);
                surface.Draw(finalSurface.SkiaSurface.Canvas, 0, 0, Surface.ReplacingPaint);
                return finalSurface;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
