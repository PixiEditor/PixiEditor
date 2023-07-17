using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.Models.Containers;

internal interface IColorsHandler
{
    public static IColorsHandler? Instance { get; }
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
}
