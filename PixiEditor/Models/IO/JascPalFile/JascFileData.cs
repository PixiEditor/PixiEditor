using SkiaSharp;

namespace PixiEditor.Models.IO.JascPalFile;

public class JascFileData
{
    public SKColor[] Colors { get; set; }

    public JascFileData(SKColor[] colors)
    {
        Colors = colors;
    }

}