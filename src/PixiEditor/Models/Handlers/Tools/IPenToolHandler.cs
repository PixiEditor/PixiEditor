namespace PixiEditor.Models.Handlers.Tools;

internal interface IPenToolHandler : IBrushToolHandler
{
    public bool PixelPerfectEnabled { get; }
}
