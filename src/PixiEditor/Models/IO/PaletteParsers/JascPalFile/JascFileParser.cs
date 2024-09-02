using System.IO;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers.JascPalFile;

/// <summary>
///     This class is responsible for parsing JASC-PAL files. Which holds the color palette data.
/// </summary>
internal class JascFileParser : PaletteFileParser
{
    private static readonly string[] _supportedFileExtensions = new string[] { ".pal", ".psppalette" };
    public override string[] SupportedFileExtensions => _supportedFileExtensions;
    public override string FileName => "Jasc Palette";

    private static async Task<PaletteFileData> ParseFile(string path)
    {
        string[] lines = await ReadTextLines(path);
        string name = Path.GetFileNameWithoutExtension(path);
        string fileType = lines[0];
        string magicBytes = lines[1];
        if (ValidateFile(fileType, magicBytes))
        {
            int colorCount = int.Parse(lines[2]);
            PaletteColor[] colors = new PaletteColor[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                string[] colorData = lines[i + 3].Split(' ');
                colors[i] = new PaletteColor(byte.Parse(colorData[0]), byte.Parse(colorData[1]), byte.Parse(colorData[2]));
            }

            return new PaletteFileData(name, colors);
        }

        throw new JascFileException("FAILED_TO_OPEN_FILE", "Invalid JASC-PAL file.");
    }

    public static async Task<bool> SaveFile(string path, PaletteFileData data)
    {
        if (data is not { Colors.Length: > 0 }) return false;

        string fileContent = "JASC-PAL\n0100\n" + data.Colors.Length;
        for (int i = 0; i < data.Colors.Length; i++)
        {
            fileContent += "\n" + data.Colors[i].R + " " + data.Colors[i].G + " " + data.Colors[i].B;
        }

        await File.WriteAllTextAsync(path, fileContent);
        return true;

    }

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

    public override async Task<bool> Save(string path, PaletteFileData data) => await SaveFile(path, data);

    private static bool ValidateFile(string fileType, string magicBytes)
    {
        return fileType.Length > 7 && fileType[..8].ToUpper() == "JASC-PAL" && magicBytes.Length > 3 && magicBytes[..4] == "0100";
    }
}
