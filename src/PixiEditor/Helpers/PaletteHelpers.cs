using PixiEditor.Models.IO;

namespace PixiEditor.Helpers
{
    public static class PaletteHelpers
    {
        public static string GetFilter(IList<PaletteFileParser> parsers, bool includeCommon)
        {
            string filter = "";

            if (includeCommon)
            {
                List<string> allSupportedFormats = new();
                foreach (var parser in parsers)
                {
                    allSupportedFormats.AddRange(parser.SupportedFileExtensions);
                }
                string allSupportedFormatsString = string.Join(';', allSupportedFormats).Replace(".", "*.");
                filter += $"Palette Files ({allSupportedFormatsString})|{allSupportedFormatsString}|";
            }

            foreach (var parser in parsers)
            {
                string supportedFormats = string.Join(';', parser.SupportedFileExtensions).Replace(".", "*.");
                filter += $"{parser.FileName} ({supportedFormats})|{supportedFormats}|";
            }

            return filter.Remove(filter.Length - 1);
        }
    }
}
