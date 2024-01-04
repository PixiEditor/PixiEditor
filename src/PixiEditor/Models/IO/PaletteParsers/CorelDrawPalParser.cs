using System.IO;
using System.Text;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class CorelDrawPalParser : PaletteFileParser
{
    public override string FileName { get; } = "CorelDRAW! 3.0 Palette";
    public override string[] SupportedFileExtensions { get; } = { ".pal" };

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
        string name = Path.GetFileNameWithoutExtension(path);

        List<PaletteColor> colors = new();

        using (Stream stream = File.OpenRead(path))
        {
            using (TextReader reader = new StreamReader(stream, Encoding.ASCII))
            {
                string line;

                while ((line = reader.ReadLine()) != null && (line.Length == 0 || line[0] != (char)26))
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        int lastQuote;
                        int numberPosition;
                        byte cyan;
                        byte magenta;
                        byte yellow;
                        byte black;

                        lastQuote = line.LastIndexOf('"');

                        if (lastQuote == -1)
                        {
                            return PaletteFileData.Corrupted; // Unable to parse line
                        }

                        numberPosition = lastQuote + 1;
                        cyan = this.NextNumber(line, ref numberPosition);
                        magenta = this.NextNumber(line, ref numberPosition);
                        yellow = this.NextNumber(line, ref numberPosition);
                        black = this.NextNumber(line, ref numberPosition);

                        colors.Add(this.ConvertCmykToRgb(cyan, magenta, yellow, black));
                    }
                }
            }
        }

        return new PaletteFileData(name, colors.ToArray());
    }

    public override bool CanSave => false;

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        throw new SavingNotSupportedException("Saving palette as CorelDRAW! 3.0 palette directly is not supported.");
    }

    private PaletteColor ConvertCmykToRgb(int c, int m, int y, int k)
    {
        int r, g, b;
        float multiplier;

        multiplier = 1 - k / 100F;

        r = Convert.ToInt32(255 * (1 - c / 100F) * multiplier);
        g = Convert.ToInt32(255 * (1 - m / 100F) * multiplier);
        b = Convert.ToInt32(255 * (1 - y / 100F) * multiplier);

        return new PaletteColor((byte)r, (byte)g, (byte)b);
    }

    private byte NextNumber(string line, ref int start)
    {
        int length;
        int valueLength;
        int maxLength;
        byte result;

        // skip any leading spaces
        while (char.IsWhiteSpace(line[start]))
        {
            start++;
        }

        length = line.Length;
        maxLength = Math.Min(3, length - start);
        valueLength = 0;

        for (int i = 0; i < maxLength; i++)
        {
            if (char.IsDigit(line[start + i]))
            {
                valueLength++;
            }
            else
            {
                break;
            }
        }

        result = byte.Parse(line.Substring(start, valueLength));

        start += valueLength;

        return result;
    }
}
