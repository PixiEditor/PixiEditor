using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
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
                Filter = SupportedFilesHelper.BuildSaveFilter(true),
                FilterIndex = 0
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
            if (Path.GetExtension(path) != Constants.NativeExtension)
            {
                var chosenFormat = ParseImageFormat(Path.GetExtension(path));
                var bitmap = document.Renderer.FinalBitmap;
                SaveAs(encodersFactory[chosenFormat](), path, bitmap.PixelWidth, bitmap.PixelHeight, bitmap);
            }
            else
            {
                Parser.PixiParser.Serialize(ParserHelpers.ToSerializable(document), path);
            }

            return path;
        }

        public static FileType ParseImageFormat(string extension)
        {
            return SupportedFilesHelper.ParseImageFormat(extension);
        }

        static Dictionary<FileType, Func<BitmapEncoder>> encodersFactory = new Dictionary<FileType, Func<BitmapEncoder>>();

        static Exporter()
        {
            encodersFactory[FileType.Png] = () => new PngBitmapEncoder();
            encodersFactory[FileType.Jpeg] = () => new JpegBitmapEncoder();
            encodersFactory[FileType.Bmp] = () => new BmpBitmapEncoder(); 
            encodersFactory[FileType.Gif] = () => new GifBitmapEncoder();
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
            if(encodersFactory.ContainsKey(info.ChosenFormat))
              SaveAs(encodersFactory[info.ChosenFormat](), info.FilePath, info.FileWidth, info.FileHeight, bitmap);
          }
        }
        public static void SaveAsGZippedBytes(string path, Surface surface)
        {
            SaveAsGZippedBytes(path, surface, SKRectI.Create(0, 0, surface.Width, surface.Height));
        }

        public static void SaveAsGZippedBytes(string path, Surface surface, SKRectI rectToSave)
        {
            var imageInfo = new SKImageInfo(rectToSave.Width, rectToSave.Height, SKColorType.RgbaF16);
            var unmanagedBuffer = Marshal.AllocHGlobal(rectToSave.Width * rectToSave.Height * 8);
            //+8 bytes for width and height
            var bytes = new byte[rectToSave.Width * rectToSave.Height * 8 + 8];
            try
            {
                surface.SkiaSurface.ReadPixels(imageInfo, unmanagedBuffer, rectToSave.Width * 8, rectToSave.Left, rectToSave.Top);
                Marshal.Copy(unmanagedBuffer, bytes, 8, rectToSave.Width * rectToSave.Height * 8);
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedBuffer);
            }
            BitConverter.GetBytes(rectToSave.Width).CopyTo(bytes, 0);
            BitConverter.GetBytes(rectToSave.Height).CopyTo(bytes, 4);
            using FileStream outputStream = new(path, FileMode.Create);
            using GZipStream compressedStream = new GZipStream(outputStream, CompressionLevel.Fastest);
            compressedStream.Write(bytes);
        }

        /// <summary>
        ///     Saves image to PNG file.
        /// </summary>
        /// <param name="encoder">encoder to do the job.</param>
        /// <param name="savePath">Save file path.</param>
        /// <param name="exportWidth">File width.</param>
        /// <param name="exportHeight">File height.</param>
        /// <param name="bitmap">Bitmap to save.</param>
        private static void SaveAs(BitmapEncoder encoder, string savePath, int exportWidth, int exportHeight, WriteableBitmap bitmap)
        {
            try
            {
                bitmap = bitmap.Resize(exportWidth, exportHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(stream);
                }
            }
            catch (Exception err)
            {
                NoticeDialog.Show(err.ToString(), "Error");
            }
        }
    }
}
