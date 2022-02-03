using PixiEditor.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PixiEditor.Helpers
{
    public class SupportedFilesHelper
    {
        static ImageFormat[] _imageFormats = new[] { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Bmp, ImageFormat.Gif, /*ImageFormat.Tiff */};
        static Dictionary<string, List<string>> extensions;
        public static ImageFormat[] ImageFormats { get => _imageFormats; }

        static SupportedFilesHelper()
        {
            extensions = new Dictionary<string, List<string>>();
            extensions[Constants.NativeExtension] = new List<string>() { Constants.NativeExtension };
            foreach(var format in _imageFormats)
                extensions[Format2Extension(format)] = GetFormatExtensions(format);
        }

        public static IEnumerable<string> GetAllSupportedExtensions()
        {
            return extensions.SelectMany(i => i.Value);
        }

        public static List<string> GetExtensions()
        {
            return extensions.Keys.ToList();
        }

        public static List<string> GetFormatExtensions(ImageFormat format)
        {
            var res = new List<string>();
            res.Add(Format2Extension(format));
            if (format == ImageFormat.Jpeg)
                res.Add(".jpg");
            return res;
        }

        public static string Format2Extension(ImageFormat format)
        {
            return "." + format.ToString().ToLower();
        }

        static string GetExtensionsFormattedForDialog(IEnumerable<string> parts)
        {
            return string.Join(";", parts.Select(i => GetExtensionFormattedForDialog(i)));
        }

        static string GetExtensionFormattedForDialog(string extension)
        {
            return "*" + extension;
        }

        public static bool IsSupportedFile(string path)
        {
            var ext = Path.GetExtension(path.ToLower());
            return GetAllSupportedExtensions().Contains(ext);
        }

        public static bool IsExtensionSupported(string fileExtension)
        {
            return GetAllSupportedExtensions().Contains(fileExtension);
        }

        public static string GetFormattedFilesExtensions(bool includePixi)
        {
            var allExts = GetAllSupportedExtensions().ToList();
            if (!includePixi)
                allExts.Remove(Constants.NativeExtension);
            var imageFilesExts = GetExtensionsFormattedForDialog(allExts);
            return imageFilesExts;
        }

        public static string BuildSaveFilter(bool includePixi)
        {
            var formatName2Extension = new Dictionary<string, string>();
            if (includePixi)
                formatName2Extension.Add("PixiEditor Files", Constants.NativeExtension);

            foreach (var format in ImageFormats)
                formatName2Extension.Add(format + " Images", Format2Extension(format));

            var filter = string.Join("|", formatName2Extension.Select(i => i.Key + "|" + GetExtensionFormattedForDialog(i.Value)));
            return filter;
        }

        public static string BuildOpenFilter()
        {
            var filter =
               "Any |" + GetFormattedFilesExtensions(true) + "|" +
               "PixiEditor Files |" + GetExtensionsFormattedForDialog(new[] { Constants.NativeExtension }) + "|" +
               "Image Files |" + GetFormattedFilesExtensions(false);

            return filter;
        }
    }
}
