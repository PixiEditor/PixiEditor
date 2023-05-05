using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.Models.IO;

internal class PaletteFileData
{
    public string Title { get; set; }
    public Color[] Colors { get; set; }
    public bool IsCorrupted { get; set; } = false;
    public static PaletteFileData Corrupted => new ("Corrupted", Array.Empty<Color>()) { IsCorrupted = true };

    public PaletteFileData(Color[] colors)
    {
        Colors = colors;
        Title = "";
    }

    public PaletteFileData(List<string> colors)
    {
        Colors = new Color[colors.Count];
        for (int i = 0; i < colors.Count; i++)
        {
            Colors[i] = Color.Parse(colors[i]);
        }

        Title = "";
    }

    public PaletteFileData(string title, Color[] colors)
    {
        Title = title;
        Colors = colors;
    }

    public string[] GetHexColors()
    {
        string[] colors = new string[Colors.Length];
        for (int i = 0; i < Colors.Length; i++)
        {
            colors[i] = Colors[i].ToString();
        }
        return colors;
    }
}
