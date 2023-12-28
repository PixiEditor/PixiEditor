using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Models.IO.PaletteParsers.Support;

internal static class ColorSpaceConverter
{
    /// <summary>
    /// Converts HSB to RGB.
    /// </summary>
    /// <param name="hue">The hue value in the range of [0, 360].</param>
    /// <param name="saturation">The saturation value in the range of [0, 1].</param>
    /// <param name="brightness">The brightness value in the range of [0, 1].</param>
    /// <returns>The HSB color converted to RGB.</returns>
    public static PaletteColor HSBToRGB(double hue, double saturation, double brightness)
    {
        double r = 0;
        double g = 0;
        double b = 0;

        if (saturation == 0)
        {
            // If saturation is 0, all colors are the same.
            // This is some flavor of gray.
            r = brightness;
            g = brightness;
            b = brightness;
        }
        else
        {
            double p;
            double q;
            double t;

            double fractionalSector;
            int sectorNumber;
            double sectorPos;

            // The color wheel consists of 6 sectors.
            // Figure out which sector you're in.
            sectorPos = hue / 60;
            sectorNumber = (int)(Math.Floor(sectorPos));

            // get the fractional part of the sector.
            // That is, how many degrees into the sector
            // are you?
            fractionalSector = sectorPos - sectorNumber;

            // Calculate values for the three axes
            // of the color.
            p = brightness * (1 - saturation);
            q = brightness * (1 - (saturation * fractionalSector));
            t = brightness * (1 - (saturation * (1 - fractionalSector)));

            // Assign the fractional colors to r, g, and b
            // based on the sector the angle is in.
            switch (sectorNumber)
            {
                case 0:
                    r = brightness;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = brightness;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = brightness;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = brightness;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = brightness;
                    break;

                case 5:
                    r = brightness;
                    g = p;
                    b = q;
                    break;
            }
        }

        // return with values scaled to be between 0 and 255
        return new PaletteColor((byte)Math.Round(b * 255.0), (byte)Math.Round(g * 255.0), (byte)Math.Round(r * 255.0));
    }

    /// <summary>
    /// Converts Lab to RGB.
    /// </summary>
    /// <param name="exL">The L component in the range of [0, 100].</param>
    /// <param name="exA">The a component in the range of [-128, 127].</param>
    /// <param name="exB">The b component in the range of [-128, 127].</param>
    /// <returns>The Lab color converted to RGB.</returns>
    public static PaletteColor LabToRGB(double exL, double exA, double exB)
    {
        int L = (int)exL;
        int a = (int)exA;
        int b = (int)exB;

        // For the conversion we first convert values to XYZ and then to RGB
        // Standards used Observer = 2, Illuminant = D65

        const double refX = 95.047;
        const double refY = 100.000;
        const double refZ = 108.883;

        double var_Y = (L + 16.0) / 116.0;
        double var_X = a / 500.0 + var_Y;
        double var_Z = var_Y - b / 200.0;

        double var_X3 = var_X * var_X * var_X;
        double var_Y3 = var_Y * var_Y * var_Y;
        double var_Z3 = var_Z * var_Z * var_Z;

        if (var_Y3 > 0.008856)
        {
            var_Y = var_Y3;
        }
        else
        {
            var_Y = (var_Y - 16 / 116) / 7.787;
        }

        if (var_X3 > 0.008856)
        {
            var_X = var_X3;
        }
        else
        {
            var_X = (var_X - 16 / 116) / 7.787;
        }

        if (var_Z3 > 0.008856)
        {
            var_Z = var_Z3;
        }
        else
        {
            var_Z = (var_Z - 16 / 116) / 7.787;
        }

        double X = refX * var_X;
        double Y = refY * var_Y;
        double Z = refZ * var_Z;

        return XYZToRGB(X, Y, Z);
    }

    /// <summary>
    /// Converts XYZ to RGB.
    /// </summary>
    /// <param name="X">The X component in the range of [0, 100].</param>
    /// <param name="Y">The Y component in the range of [0, 100].</param>
    /// <param name="Z">The Z component in the range of [0, 100].</param>
    /// <returns>PaletteColor</returns>
    private static PaletteColor XYZToRGB(double X, double Y, double Z)
    {
        // Standards used Observer = 2, Illuminant = D65
        // ref_X = 95.047, ref_Y = 100.000, ref_Z = 108.883

        double var_X = X / 100.0;
        double var_Y = Y / 100.0;
        double var_Z = Z / 100.0;

        double var_R = var_X * 3.2406 + var_Y * (-1.5372) + var_Z * (-0.4986);
        double var_G = var_X * (-0.9689) + var_Y * 1.8758 + var_Z * 0.0415;
        double var_B = var_X * 0.0557 + var_Y * (-0.2040) + var_Z * 1.0570;

        if (var_R > 0.0031308)
        {
            var_R = 1.055 * (Math.Pow(var_R, 1 / 2.4)) - 0.055;
        }
        else
        {
            var_R = 12.92 * var_R;
        }

        if (var_G > 0.0031308)
        {
            var_G = 1.055 * (Math.Pow(var_G, 1 / 2.4)) - 0.055;
        }
        else
        {
            var_G = 12.92 * var_G;
        }

        if (var_B > 0.0031308)
        {
            var_B = 1.055 * (Math.Pow(var_B, 1 / 2.4)) - 0.055;
        }
        else
        {
            var_B = 12.92 * var_B;
        }

        int nRed = (int)(var_R * 256.0);
        int nGreen = (int)(var_G * 256.0);
        int nBlue = (int)(var_B * 256.0);

        if (nRed < 0)
        {
            nRed = 0;
        }
        else if (nRed > 255)
        {
            nRed = 255;
        }

        if (nGreen < 0)
        {
            nGreen = 0;
        }
        else if (nGreen > 255)
        {
            nGreen = 255;
        }

        if (nBlue < 0)
        {
            nBlue = 0;
        }
        else if (nBlue > 255)
        {
            nBlue = 255;
        }

        return new PaletteColor((byte)nBlue, (byte)nGreen, (byte)nRed);
    }

    /// <summary>
    /// Converts CMYK to RGB.
    /// </summary>
    /// <param name="cyan">The cyan value in the range of [0, 1].</param>
    /// <param name="magenta">The magenta value in the range of [0, 1].</param>
    /// <param name="yellow">The yellow value in the range of [0, 1].</param>
    /// <param name="black">The black value in the range of [0, 1].</param>
    /// <returns>The CMYK color converted to RGB.</returns>
    public static PaletteColor CMYKToRGB(double cyan, double magenta, double yellow, double black)
    {
        double c, m, y, k;

        c = (1.0 - cyan);
        m = (1.0 - magenta);
        y = (1.0 - yellow);
        k = (1.0 - black);

        int red = (int)((1.0 - (c * (1 - k) + k)) * 255);
        int green = (int)((1.0 - (m * (1 - k) + k)) * 255);
        int blue = (int)((1.0 - (y * (1 - k) + k)) * 255);

        if (red < 0)
        {
            red = 0;
        }
        else if (red > 255)
        {
            red = 255;
        }

        if (green < 0)
        {
            green = 0;
        }
        else if (green > 255)
        {
            green = 255;
        }

        if (blue < 0)
        {
            blue = 0;
        }
        else if (blue > 255)
        {
            blue = 255;
        }

        return new PaletteColor((byte)blue, (byte)green, (byte)red);
    }
}
