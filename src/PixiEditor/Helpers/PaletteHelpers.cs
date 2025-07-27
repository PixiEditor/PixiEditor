using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Helpers;

internal static class PaletteHelpers
{
    public static List<FilePickerFileType> GetFilter(IList<PaletteFileParser> parsers, bool includeCommon)
    {
        List<FilePickerFileType> filePickerFileTypes = new();

        if (includeCommon)
        {
            List<string> allSupportedFormats = new();
            foreach (var parser in parsers)
            {
                allSupportedFormats.AddRange(parser.SupportedFileExtensions
                    .Select(x => x.Replace(".", "*.")));
            }

            string allSupportedFormatsString = string.Join(';', allSupportedFormats);
            filePickerFileTypes.Add(new FilePickerFileType($"Palette Files ({allSupportedFormatsString}")
            {
                Patterns = allSupportedFormats
            });
        }

        foreach (var parser in parsers)
        {
            filePickerFileTypes.Add(new FilePickerFileType($"{parser.FileName}")
            {
                Patterns = parser.SupportedFileExtensions.Select(x => x.Replace(".", "*.")).ToList()
            });
        }

        return filePickerFileTypes;
    }
    
    public static async Task<PaletteFileData?> GetValidParser(IList<PaletteFileParser> parsers, string fileName)
    {
        // check all parsers for formats with same file extension
        var parserList = parsers.Where(x => x.SupportedFileExtensions.Contains(Path.GetExtension(fileName).ToLower())).ToList();

        int index = 0;
        foreach (var parser in parserList)
        {
            var data = await parser.Parse(fileName);
            index++;

            if ((data.IsCorrupted || data.Colors.Length == 0) && index == parserList.Count) 
                return null; // fail if none of the parsers in our list can read the file
            if (data.IsCorrupted) 
                continue; // skip to next parser if unable to read

            return data;
        }

        return null;
    }
}
