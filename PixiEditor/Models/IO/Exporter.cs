using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.IO
{
    public class Exporter
    {
        public static string SavePath = null;
        public static Size FileDimensions;

        /// <summary>
        /// Creates ExportFileDialog to get width, height and path of file.
        /// </summary>
        /// <param name="type">Type of file to be saved in.</param>
        /// <param name="bitmap">Bitmap to be saved as file.</param>
        public static void Export(FileType type, WriteableBitmap bitmap, Size fileDimensions)
        {
            ExportFileDialog info = new ExportFileDialog(fileDimensions);
            //If OK on dialog has been clicked
            if (info.ShowDialog() == true)
            {
                //If sizes are incorrect
                if (info.FileWidth < bitmap.Width || info.FileHeight < bitmap.Height)
                {
                    MessageBox.Show("Incorrect height or width value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SavePath = info.FilePath;
                FileDimensions = new Size(info.FileWidth, info.FileHeight);
                SaveAsPng(info.FilePath, info.FileHeight, info.FileWidth, bitmap);
            }
        }

        /// <summary>
        /// Saves file with info that has been recieved from ExportFileDialog before, doesn't work without before Export() usage.
        /// </summary>
        /// <param name="type">Type of file</param>
        /// <param name="bitmap">Image to be saved as file.</param>
        public static void ExportWithoutDialog(FileType type, WriteableBitmap bitmap)
        {
            try
            {
                SaveAsPng(SavePath, (int)FileDimensions.Height, (int)FileDimensions.Width, bitmap);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Saves image to PNG file
        /// </summary>
        /// <param name="savePath">Save file path</param>
        /// <param name="exportWidth">File width</param>
        /// <param name="exportHeight">File height</param>
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
