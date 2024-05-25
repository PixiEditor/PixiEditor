using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IColorsHandler : IHandler
{
    public static IColorsHandler? Instance { get; }
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public void AddSwatch(PaletteColor paletteColor);
}
