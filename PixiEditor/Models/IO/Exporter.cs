using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Parser;
using PixiEditor.SDK;

namespace PixiEditor.Models.IO
{
    public class Exporter
    {
        /// <summary>
        ///     Saves document as .pixi file that contains all document data.
        /// </summary>
        /// <param name="document">Document to save.</param>
        /// <param name="path">Path where file was saved.</param>
        public static bool SaveAsDocumentWithDialog(Document document, out string path)
        {
            StringBuilder filter = new StringBuilder();

            foreach (Extension extension in SDKHelper.GetCurrentManager().Extensions)
            {
                if (filter.Length != 0)
                {
                    filter.Append('|');
                }

                filter.Append(extension.DisplayName);
                filter.Append('|');

                foreach (string ext in extension.SupportedDocumentFileExtensions)
                {
                    filter.Append('*');
                    filter.Append(ext);
                    filter.Append(';');
                }
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = filter.ToString(),
                DefaultExt = ".pixi"
            };
            if ((bool)dialog.ShowDialog())
            {
                path = SaveAsDocument(document, dialog.FileName);
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
        public static string SaveAsDocument(Document document, string path)
        {
            PixiParser.Serialize(document.ToSerializable(), path);
            return path;
        }

        /// <summary>
        ///     Creates ExportFileDialog to get width, height and path of file.
        /// </summary>
        /// <param name="bitmap">Bitmap to be saved as file.</param>
        /// <param name="fileDimensions">Size of file.</param>
        public static void SaveAsImage(WriteableBitmap bitmap, Size fileDimensions)
        {
            StringBuilder filter = new StringBuilder();

            foreach (Extension extension in SDKHelper.GetCurrentManager().Extensions)
            {
                if (filter.Length != 0)
                {
                    filter.Append('|');
                }

                filter.Append(extension.DisplayName);
                filter.Append('|');

                foreach (string ext in extension.SupportedImageFileExtensions)
                {
                    filter.Append('*');
                    filter.Append(ext);
                    filter.Append(';');
                }
            }

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

                SaveAsImage(info.FilePath, info.FileWidth, info.FileHeight, bitmap);
            }
        }

        /// <summary>
        ///     Saves image to PNG file.
        /// </summary>
        /// <param name="savePath">Save file path.</param>
        /// <param name="exportWidth">File width.</param>
        /// <param name="exportHeight">File height.</param>
        /// <param name="bitmap">Bitmap to save.</param>
        public static void SaveAsImage(string savePath, int exportWidth, int exportHeight, WriteableBitmap bitmap)
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