using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform.Storage;
using PixiEditor.Extensions.Palettes.Parsers;

namespace PixiEditor.AvaloniaUI.Helpers;

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

            string allSupportedFormatsString = string.Join(';', allSupportedFormats).Replace(".", "*.");
            filePickerFileTypes.Add(new FilePickerFileType($"Palette Files ({allSupportedFormatsString}")
            {
                Patterns = allSupportedFormats
            });
        }

        foreach (var parser in parsers)
        {
            string supportedFormats = string.Join(';', parser.SupportedFileExtensions).Replace(".", "*.");
            filePickerFileTypes.Add(new FilePickerFileType($"{parser.FileName} ({supportedFormats})")
            {
                Patterns = parser.SupportedFileExtensions
            });
        }

        return filePickerFileTypes;
    }
}
