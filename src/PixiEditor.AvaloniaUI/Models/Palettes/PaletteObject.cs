using System.Collections.Generic;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.AvaloniaUI.Models.Palettes;

/// <summary>
///     Class used to deserialize palette file from Lospec.
/// </summary>
internal class PaletteObject
{
    public string Name { get; set; }
    public List<string> Colors { get; set; }

    public Palette ToPalette()
    {
        List<PaletteColor> colors = new();
        foreach (string color in Colors)
        {
            Color parsedColor = Color.Parse(color);
            colors.Add(new PaletteColor(parsedColor.R, parsedColor.G, parsedColor.B));
        }

        return new(Name, colors, null, null);
    }
}
