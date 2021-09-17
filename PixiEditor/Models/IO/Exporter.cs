using Microsoft.Win32;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using SkiaSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    public class Exporter
    {
        /// <summary>
        ///     Saves document as .pixi file that contains all document data.
        /// </summary>
        /// <param name="document">Document to save.</param>
        /// <param name="path">Path where file was saved.</param>
        public static bool SaveAsEditableFileWithDialog(Document document, out string path)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PixiEditor Files | *.pixi",
                DefaultExt = "pixi"
            };
            if ((bool)dialog.ShowDialog())
            {
                path = SaveAsEditableFile(document, dialog.FileName);
                return true;
            }

            path = string.Empty;
            return false;
        }

        /// <summary>
        /// Saves editable file to chosen path and returns it.
        /// </summary>
        /// <param name="document">Document to be saved.</param>
        /// <param name="path">Path where to save file.</param>
        /// <returns>Path.</returns>
        public static string SaveAsEditableFile(Document document, string path)
        {
            Parser.PixiParser.Serialize(ParserHelpers.ToSerializable(document), path);
            return path;
        }

        /// <summary>
        ///     Creates ExportFileDialog to get width, height and path of file.
        /// </summary>
        /// <param name="bitmap">Bitmap to be saved as file.</param>
        /// <param name="fileDimensions">Size of file.</param>
        public static void Export(WriteableBitmap bitmap, Size fileDimensions)
        {
            ExportFileDialog info = new ExportFileDialog(fileDimensions);

            // If OK on dialog has been clicked
            if (info.ShowDialog())
            {
                // If sizes are incorrect
                if (info.FileWidth < bitmap.Width || info.FileHeight < bitmap.Height)
                {
                    MessageBox.Show("Incorrect height or width value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SaveAsPng(info.FilePath, info.FileWidth, info.FileHeight, bitmap);
            }
        }

        public static void SaveAsGZippedBytes(string path, Surface surface)
        {
            var imageInfo = new SKImageInfo(surface.Width, surface.Height, SKColorType.RgbaF16);
            var unmanagedBuffer = Marshal.AllocHGlobal(surface.Width * surface.Height * 8);
            //+8 bytes for width and height
            var bytes = new byte[surface.Width * surface.Height * 8 + 8];
            try
            {
                surface.SkiaSurface.ReadPixels(imageInfo, unmanagedBuffer, surface.Width * 8, 0, 0);
                Marshal.Copy(unmanagedBuffer, bytes, 8, surface.Width * surface.Height * 8);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedBuffer);
            }
            BitConverter.GetBytes((int)surface.Width).CopyTo(bytes, 0);
            BitConverter.GetBytes((int)surface.Height).CopyTo(bytes, 4);
            using FileStream outputStream = new(path, FileMode.Create);
            using GZipStream compressedStream = new GZipStream(outputStream, CompressionLevel.Fastest);
            compressedStream.Write(bytes);
        }

        /// <summary>
        ///     Saves image to PNG file.
        /// </summary>
        /// <param name="savePath">Save file path.</param>
        /// <param name="exportWidth">File width.</param>
        /// <param name="exportHeight">File height.</param>
        /// <param name="bitmap">Bitmap to save.</param>
        private static void SaveAsPng(string savePath, int exportWidth, int exportHeight, WriteableBitmap bitmap)
        {
            try
            {
                bitmap = bitmap.Resize(exportWidth, exportHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
