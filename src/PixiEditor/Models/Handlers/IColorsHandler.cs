using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using PixiEditor.Extensions.CommonApi.Palettes;

namespace PixiEditor.Models.Handlers;

internal interface IColorsHandler : IHandler
{
    public static IColorsHandler? Instance { get; }
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public bool ColorsTempSwapped { get; }
    public void AddSwatch(PaletteColor paletteColor);
}
