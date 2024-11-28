using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class GimpGplParser : PaletteFileParser
{
    public override string FileName { get; } = "GIMP Palette";
    public override string[] SupportedFileExtensions { get; } = { ".gpl" };

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

        lines = lines.Where(x => !x.StartsWith("#") && !String.Equals(x.Trim(), "GIMP Palette", StringComparison.CurrentCultureIgnoreCase)).ToArray();

        if(lines.Length == 0) return PaletteFileData.Corrupted;

        List<PaletteColor> colors = new();
        char[] separators = new[] { '\t', ' ' };
        foreach (var colorLine in lines)
        {
            var colorParts = colorLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if (colorParts.Length < 3)
            {
                continue;
            }

            if(colorParts.Length < 3) continue;

            bool parsed = false;

            parsed = byte.TryParse(colorParts[0], out byte r);
            if(!parsed) continue;

            parsed = byte.TryParse(colorParts[1], out byte g);
            if(!parsed) continue;

            parsed = byte.TryParse(colorParts[2], out byte b);
            if(!parsed) continue;

            var color = new PaletteColor(r, g, b); // alpha is ignored in PixiEditor
            if (colors.Contains(color)) continue;

            colors.Add(color);
        }

        return new PaletteFileData(name, colors.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        StringBuilder sb = new();
        string name = string.IsNullOrEmpty(data.Title) ? Path.GetFileNameWithoutExtension(path) : data.Title;
        sb.AppendLine("GIMP Palette");
        sb.AppendLine($"#Name: {name}");
        sb.AppendLine($"#Colors {data.Colors.Length}");
        sb.AppendLine("#Made with PixiEditor");
        sb.AppendLine("#");
        foreach (var color in data.Colors)
        {
            string hex = $"{color.R:X}{color.G:X}{color.B:X}";
            sb.AppendLine($"{color.R}\t{color.G}\t{color.B}\t{hex}");
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
