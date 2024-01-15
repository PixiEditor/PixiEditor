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
        return new PaletteColor((byte)Math.Round(r * 255.0), (byte)Math.Round(g * 255.0), (byte)Math.Round(b * 255.0));
    }

    /// <summary>
    /// Converts Lab to RGB.
    /// </summary>
    /// <param name="L">The L component in the range of [0, 100].</param>
    /// <param name="a">The a component in the range of [-128, 127].</param>
    /// <param name="b">The b component in the range of [-128, 127].</param>
    /// <returns>The Lab color converted to RGB.</returns>
    public static PaletteColor LabToRGB(double L, double a, double b)
    {
        // For the conversion we first convert values to XYZ and then to RGB
        // Standards used Observer = 2, Illuminant = D50 (through experimentation I figured out that photoshop uses D50 instead of the standard D65)

        const double refX = 96.4212;
        const double refY = 100.000;
        const double refZ = 82.5188;

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
            var_Y = (var_Y - 16.0 / 116) / 7.787;
        }

        if (var_X3 > 0.008856)
        {
            var_X = var_X3;
        }
        else
        {
            var_X = (var_X - 16.0 / 116) / 7.787;
        }

        if (var_Z3 > 0.008856)
        {
            var_Z = var_Z3;
        }
        else
        {
            var_Z = (var_Z - 16.0 / 116) / 7.787;
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
        // Standards used Observer = 2, Illuminant = D50 (through experimentation I figured out that photoshop uses D50 instead of the standard D65)
        // refX = 96.4212
        // refY = 100.000
        // refZ = 82.5188

        // Reference white points for D50 Illuminat, 10° observer
        // X = 0.34773, Y = 0.35952
        // https://en.wikipedia.org/wiki/Standard_illuminant

        double var_X = X / 100.0;
        double var_Y = Y / 100.0;
        double var_Z = Z / 100.0;

        // Source for matrix used: http://www.brucelindbloom.com/index.html?Eqn_RGB_XYZ_Matrix.html
        double var_R = var_X * 3.1338561 + var_Y * (-1.6168667) + var_Z * (-0.4906146);
        double var_G = var_X * (-.9787684) + var_Y * 1.9161415 + var_Z * 0.0334540;
        double var_B = var_X * 0.0719453 + var_Y * (-0.2289914) + var_Z * 1.4052427;

        var_R = GammaTransform(var_R);
        var_G = GammaTransform(var_G);
        var_B = GammaTransform(var_B);

        int nRed = (int)Math.Round(var_R * 255.0);
        int nGreen = (int)Math.Round(var_G * 255.0);
        int nBlue = (int)Math.Round(var_B * 255.0);

        nRed = Clamp(nRed);
        nGreen = Clamp(nGreen);
        nBlue = Clamp(nBlue);

        return new PaletteColor((byte)nRed, (byte)nGreen, (byte)nBlue);
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

        // The following Adobe ICC profile would do a great job here: WebCoatedSWOP2006Grade3.icc
        //
        // The license conditions for profile are a bit vague, so I don't use it.
        //
        // There is a very good replacement from color.org named SWOP2006_Coated3v2.icc and its free.
        // Let it be known though that CMYK - > RGB conversion can not be done with complete accuracy
        // (with any ICC profile) because of the nature of the 2 color spaces. We can at least give a
        // good guess about the color.

        // var cmykColor = new float[] { (float)cyan, (float)magenta, (float)yellow, (float)black };

        // var newColor = Color.FromValues(cmykColor, new Uri("C:\\ICC Profiles\\SWOP2006_Coated3v2.icc"));

        // return new PaletteColor(newColor.R, newColor.G, newColor.B);

        // The following magic formulas emulate the Adobe ICC profile. It's not perfect but better
        // than the simple conversion formula (1 - c/m/y)*(1 - k) found overall the internet. 
        // The formulas and their factors were found by experiments in color wizardry. 

        double r = 1 - ((0.2 * c + 1 - 0.25 * m - 0.25 * y - 0.7 * k) * c
                        + (0.09 + 0.08 * y - 0.05 * k) * m
                        + (-0.05 + 0.05 * k) * y
                        + (0.15 * k + 0.7) * k);
        double g = 1 - ((0.34 - 0.3 * m - 0.18 * k) * c
                        + (0.1 * m + 0.8 - 0.05 * y - 0.62 * k) * m
                        + (0.1 - 0.1 * k) * y
                        + (0.15 * k + 0.7) * k);
        double b = 1 - ((0.09 - 0.1 * m - 0.1 * y) * c
                        + (0.48 - 0.3 * y - 0.2 * k) * m
                        + (0.1 * y + 0.8 - 0.74 * k) * y
                        + (0.15 * k + 0.7) * k);

        return new PaletteColor(ClampColorByte(r), ClampColorByte(g), ClampColorByte(b));
    }

    /// <summary>
    /// Applies gamma correction to a linear RGB component in XYZ conversion. 
    /// https://en.wikipedia.org/wiki/SRGB#Specification_of_the_transformation
    /// </summary>
    /// <param name="linearVal">The linear color component to transform</param>
    /// <returns>The RGB component converted to non-linear</returns>
    private static double GammaTransform(double linearVal)
    {
        if (linearVal < 0.0031308)
        {
            return 12.92 * linearVal;

        }
        return 1.055 * (Math.Pow(linearVal, 1 / 2.4)) - 0.055;
    }

    /// <summary>
    /// Clamp a value to 0-255
    /// </summary>
    private static int Clamp(int i)
    {
        if (i < 0) return 0;
        if (i > 255) return 255;
        return i;
    }

    /// <summary>
    /// Convert the relative color in range [0..1] to [0..255] and check the limits
    /// </summary>
    /// <param name="relativeColor"></param>
    /// <returns></returns>
    private static byte ClampColorByte(double relativeColor)
    {
        byte colorValue;

        relativeColor = relativeColor * 255;

        if (relativeColor > 255)
        {
            colorValue = 255;
        }
        else if (relativeColor < 0)
        {
            colorValue = 0;
        }
        else
        {
            colorValue = (byte)relativeColor;
        }

        return colorValue;
    }
}
