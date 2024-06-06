namespace PixiEditor.Extensions.CommonApi.Palettes.Parsers;

public class PaletteFileData
{
    public string Title { get; set; }
    public PaletteColor[] Colors { get; set; }
    public bool IsCorrupted { get; set; } = false;
    public static PaletteFileData Corrupted => new ("Corrupted", Array.Empty<PaletteColor>()) { IsCorrupted = true };

    public PaletteFileData(PaletteColor[] colors)
    {
        Colors = colors;
        Title = "";
    }

    public PaletteFileData(List<string> colors)
    {
        Colors = new PaletteColor[colors.Count];
        for (int i = 0; i < colors.Count; i++)
        {
            Colors[i] = PaletteColor.Parse(colors[i]);
        }

        Title = "";
    }

    public PaletteFileData(string title, PaletteColor[] colors)
    {
        Title = title;
        Colors = colors;
    }

    public PaletteColor[] GetPaletteColors()
    {
        PaletteColor[] colors = new PaletteColor[Colors.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            PaletteColor color = Colors[i];
            colors[i] = new PaletteColor((byte)color.R, (byte)color.G, (byte)color.B);
        }

        return colors;
    }
}
