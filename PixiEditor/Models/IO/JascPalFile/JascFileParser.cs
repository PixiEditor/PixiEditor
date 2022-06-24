using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

namespace PixiEditor.Models.IO.JascPalFile;

/// <summary>
///     This class is responsible for parsing JASC-PAL files. Which holds the color palette data.
/// </summary>
public class JascFileParser : PaletteFileParser
{
    private static readonly string[] _supportedFileExtensions = new string[] { ".pal" };
    public override string[] SupportedFileExtensions => _supportedFileExtensions;
    public override string FileName => "Jasc Palette";

    public static async Task<PaletteFileData> ParseFile(string path)
    { 
        using var stream = File.OpenText(path);
        
        string fileContent = await stream.ReadToEndAsync();
        string[] lines = fileContent.Split('\n');
        string name = Path.GetFileNameWithoutExtension(path);
        string fileType = lines[0];
        string magicBytes = lines[1];
        if (ValidateFile(fileType, magicBytes))
        {
            int colorCount = int.Parse(lines[2]);
            SKColor[] colors = new SKColor[colorCount];
            for (int i = 0; i < colorCount; i++)
            {
                string[] colorData = lines[i + 3].Split(' ');
                colors[i] = new SKColor(byte.Parse(colorData[0]), byte.Parse(colorData[1]), byte.Parse(colorData[2]));
            }

            return new PaletteFileData(name, colors);
        }

        throw new JascFileException("Invalid JASC-PAL file.");
    }

    public static async Task SaveFile(string path, PaletteFileData data)
    {
        string fileContent = "JASC-PAL\n0100\n" + data.Colors.Length;
        for (int i = 0; i < data.Colors.Length; i++)
        {
            fileContent += "\n" + data.Colors[i].Red + " " + data.Colors[i].Green + " " + data.Colors[i].Blue;
        }

        await File.WriteAllTextAsync(path, fileContent);
    }

    public override async Task<PaletteFileData> Parse(string path) => await ParseFile(path);

    public override async Task Save(string path, PaletteFileData data) => await SaveFile(path, data);

    private static bool ValidateFile(string fileType, string magicBytes)
    {
        return fileType.Length > 7 && fileType[..8].ToUpper() == "JASC-PAL" && magicBytes.Length > 3 && magicBytes[..4] == "0100";
    }
}