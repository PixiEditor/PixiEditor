using System.IO;
using SkiaSharp;

namespace PixiEditor.Models.IO.JascPalFile;

/// <summary>
///     This class is responsible for parsing JASC-PAL files. Which holds the color palette data.
/// </summary>
public static class JascFileParser
{
    public static JascFileData Parse(string path)
    {
        string fileContent = File.ReadAllText(path);
        string[] lines = fileContent.Split('\n');
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

            return new JascFileData(colors);
        }

        throw new JascFileException("Invalid JASC-PAL file.");
    }

    public static void Save(string path, JascFileData data)
    {
        string fileContent = "JASC-PAL\n0100\n" + data.Colors.Length;
        for (int i = 0; i < data.Colors.Length; i++)
        {
            fileContent += "\n" + data.Colors[i].Red + " " + data.Colors[i].Green + " " + data.Colors[i].Blue;
        }

        File.WriteAllText(path, fileContent);
    }

    private static bool ValidateFile(string fileType, string magicBytes)
    {
        return fileType.Length > 7 && fileType[..8].ToUpper() == "JASC-PAL" && magicBytes.Length > 3 && magicBytes[..4] == "0100";
    }
}