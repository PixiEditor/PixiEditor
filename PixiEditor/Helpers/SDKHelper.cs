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
        }
    }
}
