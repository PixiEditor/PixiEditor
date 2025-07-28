using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;
using PixiEditor.Helpers;

namespace PixiEditor.Models.IO.PaletteParsers;

// https://www.getpaint.net/doc/latest/WorkingWithPalettes.html

internal class PaintNetTxtParser : PaletteFileParser
{
    public override string FileName { get; } = "Paint.NET Palette";
    public override string[] SupportedFileExtensions { get; } = new string[] { ".txt" };
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

    private static async Task<PaletteFileData> ParseFile(string path)
    {
        var lines = await ReadTextLines(path);
        string name = Path.GetFileNameWithoutExtension(path);

        lines = lines.Where(x => !x.StartsWith(";")).ToArray();

        List<PaletteColor> colors = new();
        for (int i = 0; i < lines.Length; i++)
        {
            // Color format aarrggbb
            string colorLine = lines[i];
            if(colorLine.Length < 8)
                continue;

            byte a = byte.Parse(colorLine.Substring(0, 2), NumberStyles.HexNumber);
            byte r = byte.Parse(colorLine.Substring(2, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(colorLine.Substring(4, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(colorLine.Substring(6, 2), NumberStyles.HexNumber);
            var color = new PaletteColor(r, g, b); // alpha is ignored in PixiEditor
            if(colors.Contains(color)) continue;

            colors.Add(color);
        }

        return new PaletteFileData(name, colors.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("; Paint.NET Palette File");
        sb.AppendLine($"; Made using PixiEditor {VersionHelpers.GetCurrentAssemblyVersion().ToString()}");
        sb.AppendLine($"; {data.Colors.Length} colors");
        foreach (PaletteColor color in data.Colors)
        {
            sb.AppendLine($"FF{color.R:X2}{color.G:X2}{color.B:X2}");
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
