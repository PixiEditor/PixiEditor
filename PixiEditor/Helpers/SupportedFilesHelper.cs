using PixiEditor.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;


namespace PixiEditor.Helpers
{
    internal class SupportedFilesHelper
    {
        static ImageFormat[] _imageFormats = new[] { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Gif, /*ImageFormat.Tiff */};
        static Dictionary<string, List<string>> extensions;
        public static ImageFormat[] ImageFormats { get => _imageFormats; }

        static SupportedFilesHelper()
        {
            extensions = new Dictionary<string, List<string>>();
            extensions[Constants.NativeExtension] = new List<string>() { Constants.NativeExtension };
            foreach(var format in _imageFormats)
                extensions[Format2Extension(format)] = GetExtensions(format);
        }

        public static IEnumerable<string> GetSupportedExtensions()
        {
            return extensions.SelectMany(i => i.Value);
        }

        public static List<string> GetExtensions(ImageFormat format)
        {
            var res = new List<string>();
            res.Add(Format2Extension(format));
            if (format == ImageFormat.Jpeg)
                res.Add(".jpg");
            return res;
        }

        private static string Format2Extension(ImageFormat format)
        {
            return "." + format.ToString().ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageFormat">ImageFormat</param>
        /// <returns>Extensions of the image e.g. *.jpg;*.jpeg</returns>
        public static string GetExtensionsFormattedForDialog(ImageFormat imageFormat)
        {
            var parts = GetExtensions(imageFormat);
            return GetExtensionsFormattedForDialog(parts);
        }

        public static string GetExtensionsFormattedForDialog(IEnumerable<string> parts)
        {
            return string.Join(";", parts.Select(i => "*" + i));
        }

        public static bool IsSupportedFile(string path)
        {
            var ext = Path.GetExtension(path.ToLower());
            return GetSupportedExtensions().Contains(ext);
        }

        public static bool IsExtensionSupported(string fileExtension)
        {
            return GetSupportedExtensions().Contains(fileExtension);
        }

        public static string GetFormattedFilesExtensions(bool includePixi)
        {
            var allExts = SupportedFilesHelper.GetSupportedExtensions().ToList();
            if (!includePixi)
                allExts.Remove(Constants.NativeExtension);
            var imageFilesExts = SupportedFilesHelper.GetExtensionsFormattedForDialog(allExts);
            return imageFilesExts;
        }
    }
}
