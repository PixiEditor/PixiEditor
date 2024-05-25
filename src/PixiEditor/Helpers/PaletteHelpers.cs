using System.IO;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Models.AppExtensions.Services;
using PixiEditor.Models.IO;

namespace PixiEditor.Helpers;

internal static class PaletteHelpers
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

    public static async Task<PaletteFileData> GetValidParser(IList<PaletteFileParser> parsers, string fileName)
    {
        // check all parsers for formats with same file extension
        var parserList = parsers.Where(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName).ToLower())).ToList();

        if (parserList != null)
        {
            int index = 0;
            foreach (var parser in parserList)
            {
                var data = await parser.Parse(fileName);
                index++;

                if ((data.IsCorrupted || data.Colors.Length == 0) && index == parserList.Count) return null; // fail if none of the parsers in our list can read the file
                if (data.IsCorrupted) continue; // skip to next parser if unable to read

                return data;
            }
        }

        return null;
    }
}
