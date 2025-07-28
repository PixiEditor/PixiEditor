using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class HexPaletteParser : PaletteFileParser
{
    public override string FileName { get; } = "Hex Palette";
    public override string[] SupportedFileExtensions { get; } = { ".hex" };
    public override async Task<PaletteFileData> Parse(string path)
    {
        try
        {
            return await ParseFile(path);
        }
        catch
        {
            return PaletteFileData.Corrupted;
        }
    }

    private async Task<PaletteFileData> ParseFile(string path)
    {
        var lines = await ReadTextLines(path);
        string name = Path.GetFileNameWithoutExtension(path);

        List<PaletteColor> colors = new();
        foreach (var colorLine in lines)
        {
            if (colorLine.Length < 6)
                continue;

            byte r = byte.Parse(colorLine.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(colorLine.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(colorLine.Substring(4, 2), NumberStyles.HexNumber);
            var color = new PaletteColor(r, g, b); // alpha is ignored in PixiEditor
            if (colors.Contains(color)) continue;

            colors.Add(color);
        }

        return new PaletteFileData(name, colors.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        StringBuilder sb = new();
        foreach (var color in data.Colors)
        {
            string hex = $"{color.R:X2}{color.G:X2}{color.B:X2}";
            sb.AppendLine(hex);
        }

        try
        {
            await File.WriteAllTextAsync(path, sb.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
