using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Extensions.CommonApi.Palettes.Parsers;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class CorelDrawPalParser : PaletteFileParser
{
    public override string FileName => "CorelDRAW! 3.0 Palette";
    public override string[] SupportedFileExtensions { get; } = { ".pal" };

    // Default name to use for colors (color name is required by format)
    private const string SWATCH_NAME = "PixiEditor Color";

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
                string? line;

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

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        StringBuilder sb = new StringBuilder(SWATCH_NAME.Length + 20);

        try
        {
            await using (Stream stream = File.OpenWrite(path))
            {
                using (TextWriter writer = new StreamWriter(stream, Encoding.ASCII, 1024, true))
                {
                    foreach (var color in data.Colors)
                    {
                        this.ConvertRgbToCmyk(color, out byte c, out byte m, out byte y, out byte k);

                        this.WriteName(sb, SWATCH_NAME);
                        this.WriteNumber(sb, c);
                        this.WriteNumber(sb, m);
                        this.WriteNumber(sb, y);
                        this.WriteNumber(sb, k);

                        writer.WriteLine(sb.ToString());
                        sb.Length = 0;
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static byte ClampCmyk(float value)
    {
        if (value < 0 || float.IsNaN(value))
        {
            value = 0;
        }

        return Convert.ToByte(value * 100);
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

    private void ConvertRgbToCmyk(PaletteColor color, out byte c, out byte m, out byte y, out byte k)
    {
        float r, g, b;
        float divisor;

        r = color.R / 255F;
        g = color.G / 255F;
        b = color.B / 255F;

        divisor = 1 - Math.Max(Math.Max(r, g), b);

        c = ClampCmyk((1 - r - divisor) / (1 - divisor));
        m = ClampCmyk((1 - g - divisor) / (1 - divisor));
        y = ClampCmyk((1 - b - divisor) / (1 - divisor));
        k = ClampCmyk(divisor);
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

    private void WriteName(StringBuilder sb, string name)
    {
        sb.Append('"');
        sb.Append(name);
        sb.Append('"');

        for (int j = name.Length; j < name.Length; j++)
        {
            sb.Append(' ');
        }
    }

    private void WriteNumber(StringBuilder sb, byte value)
    {
        if (value == 100)
        {
            sb.Append("100 ");
        }
        else
        {
            sb.Append(' ');
            if (value < 10)
            {
                sb.Append(' ');
            }

            sb.Append(value);

            sb.Append(' ');
        }
    }
}
