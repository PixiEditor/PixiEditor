using System.Diagnostics;
using System.Drawing;
using System.IO;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Extensions.Palettes.Parsers;
using PixiEditor.Models.IO.PaletteParsers.Support;

namespace PixiEditor.Models.IO.PaletteParsers;

// Implementation based on: 
// https://devblog.cyotek.com/post/reading-photoshop-color-swatch-aco-files-using-csharp
// Copyright © Richard Moss, licensed under MIT.

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
                    short[] colorValues = new short[4];

                    swatchIndex++;

                    // only write RGB colorspace because that's all we support currently
                    colorValues[0] = (short)(color.R * 256);
                    colorValues[1] = (short)(color.G * 256);
                    colorValues[2] = (short)(color.B * 256);
                    colorValues[3] = 0;

                    AdobeFileSupport.WriteInt16(stream, (short)AdobeColorSpace.Rgb);
                    AdobeFileSupport.WriteInt16(stream, colorValues[0]);
                    AdobeFileSupport.WriteInt16(stream, colorValues[1]);
                    AdobeFileSupport.WriteInt16(stream, colorValues[2]);
                    AdobeFileSupport.WriteInt16(stream, colorValues[3]);

                    // write out a generic name just for v2 compatibility even though we don't support named colors
                    string name;
                    name = string.Format("Swatch {0}", swatchIndex);

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

        string swatchName = string.Empty;

        // read the number of colors, which also occupies two bytes
        colorCount = AdobeFileSupport.ReadInt16(stream);

        for (int i = 0; i < colorCount; i++)
        {
            AdobeColorSpace colorSpace;
            int[] colorValues = new int[4];

            // again, two bytes for the color space
            colorSpace = (AdobeColorSpace)(AdobeFileSupport.ReadInt16(stream));

            colorValues[0] = AdobeFileSupport.ReadInt16(stream);
            colorValues[1] = AdobeFileSupport.ReadInt16(stream);
            colorValues[2] = AdobeFileSupport.ReadInt16(stream);
            colorValues[3] = AdobeFileSupport.ReadInt16(stream);

            if (version == ACOFileVersion.Version2)
            {
                // need to read the name even our color collection doesn't support individual names
                int length = AdobeFileSupport.ReadInt32(stream);
                swatchName = AdobeFileSupport.ReadString(stream, length);
            }

            PaletteColor color = PaletteColor.Empty;
            switch (colorSpace)
            {
                case AdobeColorSpace.Rgb:
                    // RGB. The specific variant of RGB is not specified, so this can be sRGB, Adobe RGB, Apple RGB, etc. We interpret it as sRGB
                    // The first three values in the color data are red, green, and blue. They are full unsigned
                    // 16-bit values as in Apple's RGBColor data structure. Pure red = 65535, 0, 0.

                    int red = colorValues[0] / 256;
                    int green = colorValues[1] / 256;
                    int blue = colorValues[2] / 256;

                    color = new PaletteColor((byte)red, (byte)green, (byte)blue);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Hsb:
                    // HSB.
                    // The first three values in the color data are hue , saturation , and brightness . They are full 
                    // unsigned 16-bit values as in Apple's HSVColor data structure. Pure red = 0,65535, 65535.

                    double hue = (colorValues[0] / 65535.0) * 360.0;
                    double saturation = (colorValues[1] / 65535.0);
                    double brightness = (colorValues[2] / 65535.0);

                    color = ColorSpaceConverter.HSBToRGB(hue, saturation, brightness);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Cmyk:
                    // CMYK.
                    // The first four values in the color data are cyan, magenta, yellow, and black. They are full 
                    // unsigned 16-bit values. Pure cyan = 0,65535,65535,65535.

                    double cyan = (colorValues[0] / 65535.0);
                    double magenta = (colorValues[1] / 65535.0);
                    double yellow = (colorValues[2] / 65535.0);
                    double black = (colorValues[3] / 65535.0);

                    color = ColorSpaceConverter.CMYKToRGB(cyan, magenta, yellow, black);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Lab:

                    double L = BitConverter.ToInt16(BitConverter.GetBytes(colorValues[0])) / 100.0;
                    double a = BitConverter.ToInt16(BitConverter.GetBytes(colorValues[1])) / 100.0;
                    double b = BitConverter.ToInt16(BitConverter.GetBytes(colorValues[2])) / 100.0;

                    color = ColorSpaceConverter.LabToRGB(L, a, b);

                    results.Add(color);
                    break;

                case AdobeColorSpace.Grayscale:
                    // Grayscale.
                    // The first value in the color data is the gray value, from 0...10000.

                    byte gray = (byte)((colorValues[0] / 10000.0) * 255);

                    color = new PaletteColor(gray, gray, gray);

                    results.Add(color);
                    break;

                default:
                    throw new InvalidDataException(string.Format("Color space '{0}' not supported.", colorSpace));
            }
#if DEBUG
            if (version == ACOFileVersion.Version2)
                Debug.WriteLine("Name: {0}, Mode: {1}, Color: {2}", swatchName, colorSpace, color.Hex);
            else
                Debug.WriteLine("Mode: {0}, Color: {1}", colorSpace, color.Hex);
#endif
        }

        return results;
    }
}
