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
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            try
            {
                return SDKHelper.FileParsers.CreateImageParser(Path.GetExtension(path), stream).Parse();
            }
            catch (Exception e)
            {
                throw new CorruptedFileException("Selected file is invalid or corrupted.", e);
            }
        }

        public static SerializableDocument ImportSerializeDocument(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            try
            {
                var document = SDKHelper.FileParsers.CreateDocumentParser(Path.GetExtension(path), stream).Parse();

                return document;
            }
            catch (Exception e)
            {
                throw new CorruptedFileException("Selected file is invalid or corrupted.", e);
            }
        }

        public static Document ImportDocument(string path)
        {
            Document document = ImportSerializeDocument(path).ToDocument();
            document.DocumentFilePath = path;

            return document;
        }

        public static bool IsSupportedFile(string path)
        {
            return SDKHelper.GetCurrentManager().Parsers.SupportedExtensions.Contains(Path.GetExtension(path));
        }
    }
}