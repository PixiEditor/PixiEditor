using System.Drawing;
using System.IO;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Models.IO.PaletteParsers.Support;

namespace PixiEditor.Models.IO.PaletteParsers;

internal class PhotoshopColorSwatchParser : PaletteFileParser
{
    public override string FileName { get; } = "Adobe Photoshop Color Swatch";
    public override string[] SupportedFileExtensions { get; } = { ".aco" };


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
        List<PaletteColor> colorPalette;
        string name = Path.GetFileNameWithoutExtension(path);

        await using (Stream stream = File.OpenRead(path))
        {
            ACOFileVersion version;

            // read the version, which occupies two bytes
            version = (ACOFileVersion)AdobeFileSupport.ReadInt16(stream);

            if (version != ACOFileVersion.Version1 && version != ACOFileVersion.Version2)
                return PaletteFileData.Corrupted; // Invalid version information

            // the specification states that a version2 palette follows a version1
            // the only difference between version1 and version2 is the inclusion 
            // of a name property. Perhaps there's addtional color spaces as well
            // but we can't support them all anyway
            // I noticed some files no longer include a version 1 palette

            colorPalette = this.ReadSwatches(stream, version);
            if (version == ACOFileVersion.Version1)
            {
                version = (ACOFileVersion)AdobeFileSupport.ReadInt16(stream);
                if (version == ACOFileVersion.Version2)
                    colorPalette = this.ReadSwatches(stream, version);
            }
        }

        return new PaletteFileData(name, colorPalette.ToArray());
    }

    public override async Task<bool> Save(string path, PaletteFileData data)
    {
        try
        {
            await using (Stream stream = File.OpenWrite(path))
            {
                int swatchIndex;

                AdobeFileSupport.WriteInt16(stream, (short)ACOFileVersion.Version2);
                AdobeFileSupport.WriteInt16(stream, (short)data.Colors.Count());

                swatchIndex = 0;

                foreach (PaletteColor color in data.Colors)
                {
                    short value1;
                    short value2;
                    short value3;
                    short value4;

                    swatchIndex++;

                    // only write RGB colorspace because that's all we support currently
                    value1 = (short)(color.R * 256);
                    value2 = (short)(color.G * 256);
                    value3 = (short)(color.B * 256);
                    value4 = 0;

                    AdobeFileSupport.WriteInt16(stream, (short)AdobeColorSpace.Rgb);
                    AdobeFileSupport.WriteInt16(stream, value1);
                    AdobeFileSupport.WriteInt16(stream, value2);
                    AdobeFileSupport.WriteInt16(stream, value3);
                    AdobeFileSupport.WriteInt16(stream, value4);

                    // write out a generic name just for v2 compatibility even though we don't support named colors
                    string name;
                    name = string.Format("PixiEditor Swatch {0}", swatchIndex);

                    AdobeFileSupport.WriteInt32(stream, name.Length);
                    AdobeFileSupport.WriteString(stream, name);

                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<PaletteColor> ReadSwatches(Stream stream, ACOFileVersion version)
    {
        int colorCount;
        List<PaletteColor> results;

        results = new List<PaletteColor>();

        // read the number of colors, which also occupies two bytes
        colorCount = AdobeFileSupport.ReadInt16(stream);

        for (int i = 0; i < colorCount; i++)
        {
            AdobeColorSpace colorSpace;
            int value1;
            int value2;
            int value3;
            int value4;

            // again, two bytes for the color space
            colorSpace = (AdobeColorSpace)(AdobeFileSupport.ReadInt16(stream));

            value1 = AdobeFileSupport.ReadInt16(stream);
            value2 = AdobeFileSupport.ReadInt16(stream);
            value3 = AdobeFileSupport.ReadInt16(stream);
            value4 = AdobeFileSupport.ReadInt16(stream);

            if (version == ACOFileVersion.Version2)
            {
                // need to read the name even our color collection doesn't support individual names
                int length = AdobeFileSupport.ReadInt32(stream);
                AdobeFileSupport.ReadString(stream, length);
            }

            PaletteColor color = PaletteColor.Empty;
            Color newColor = Color.Empty;
            switch (colorSpace)
            {
                case AdobeColorSpace.Rgb:
                    // RGB.
                    // The first three values in the color data are red, green, and blue. They are full unsigned
                    // 16-bit values as in Apple's RGBColor data structure. Pure red = 65535, 0, 0.

                    int red = value1 / 256; // 0-255
                    int green = value2 / 256; // 0-255
                    int blue = value3 / 256; // 0-255

                    color = new PaletteColor((byte)red, (byte)green, (byte)blue);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Hsb:
                    // HSB.
                    // The first three values in the color data are hue , saturation , and brightness . They are full 
                    // unsigned 16-bit values as in Apple's HSVColor data structure. Pure red = 0,65535, 65535.

                    double hue = value1 / 182.04; // 0-359
                    double saturation = value2 / 655.35; // 0-100
                    double brightness = value3 / 655.35; // 0-100

                    newColor = new HslColor(hue, saturation, brightness).ToRgbColor();
                    color = new PaletteColor(newColor.R, newColor.G, newColor.B);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Cmyk:
                    // CMYK.
                    // The first four values in the color data are cyan, magenta, yellow, and black. They are full 
                    // unsigned 16-bit values. Pure cyan = 0,65535,65535,65535.

                    double cyan = 100 - (value1 / 655.35);
                    double magenta = 100 - (value2 / 655.35);
                    double yellow = 100 - (value3 / 655.35);
                    double black = 100 - (value4 / 655.35);

                    newColor = new CmykColor(cyan, magenta, yellow, black).ToRgbColor();
                    color = new PaletteColor(newColor.R, newColor.G, newColor.B);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Lab:
                    double L = value1 / 100;
                    double a = value2 / 100;
                    double b = value3 / 100;

                    newColor = new CIELabColor(L, a, b).ToRgbColor();
                    color = new PaletteColor(newColor.R, newColor.G, newColor.B);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Grayscale:
                    // Grayscale.
                    // The first value in the color data is the gray value, from 0...10000.

                    int gray = (int)(value1 / 39.0625); // 0-255

                    color = new PaletteColor((byte)gray, (byte)gray, (byte)gray);

                    results.Add(color);
                    break;

                default:
                    throw new InvalidDataException(string.Format("Color space '{0}' not supported.", colorSpace));
            }
        }

        return results;
    }
}
