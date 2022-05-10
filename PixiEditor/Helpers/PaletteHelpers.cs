using System.Collections.Generic;
using PixiEditor.Models.IO;

namespace PixiEditor.Helpers
{
    public static class PaletteHelpers
    {
        public static string GetFilter(IList<PaletteFileParser> parsers)
        {
            string filter = "";

            foreach (var parser in parsers)
            {
                string supportedFormats = string.Join(';', parser.SupportedFileExtensions).Replace(".", "*.");
                filter += $"{parser.FileName} ({supportedFormats})|{supportedFormats}|";
            }

            return filter.Remove(filter.Length - 1);
        }
    }
}
