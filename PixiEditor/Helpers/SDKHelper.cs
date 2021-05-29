using PixiEditor.SDK;
using PixiEditor.SDK.FileParsers;
using PixiEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Helpers
{
    public static class SDKHelper
    {
        internal static SDKManager GetCurrentManager()
        {
            return ViewModelMain.Current?.ExtensionSubViewModel?.SDKManager;
        }

        public static class FileParsers
        {
            public static DocumentParser CreateDocumentParser(string extension, Stream stream)
            {
                return GetCurrentManager().Parsers.CreateDocumentParser(extension, stream);
            }

            public static ImageParser CreateImageParser(string extension, Stream stream)
            {
                return GetCurrentManager().Parsers.CreateImageParser(extension, stream);
            }

            public static bool HasDocumentParser(string extension) => GetCurrentManager().Parsers.SupportedDocumentExtensions.Contains(extension);

            public static bool HasImageParser(string extension) => GetCurrentManager().Parsers.SupportedImageExtensions.Contains(extension);

            public static string GetFileFilter()
            {
                FileFilterBuilder builder = new FileFilterBuilder();

                SDKManager manager = GetCurrentManager();

                builder.AddFilter("Documents", manager.Parsers.SupportedDocumentExtensions);
                builder.AddFilter("Images", manager.Parsers.SupportedImageExtensions);

                foreach (Extension extension in manager.Extensions)
                {
                    builder.AddFilter($"{extension.DisplayName} Documents", extension.SupportedDocumentFileExtensions);
                    builder.AddFilter($"{extension.DisplayName} Images", extension.SupportedImageFileExtensions);
                }

                return builder.Build(true);
            }

            public static string GetDocumentFilter()
            {
                FileFilterBuilder builder = new FileFilterBuilder();

                foreach (Extension extension in GetCurrentManager().Extensions)
                {
                    builder.AddFilter(extension.DisplayName, extension.SupportedDocumentFileExtensions);
                }

                return builder.Build(true);
            }

            public static string GetImageFilter()
            {
                FileFilterBuilder builder = new FileFilterBuilder();

                foreach (Extension extension in GetCurrentManager().Extensions)
                {
                    builder.AddFilter(extension.DisplayName, extension.SupportedImageFileExtensions);
                }

                return builder.Build(true);
            }
        }
    }
}
