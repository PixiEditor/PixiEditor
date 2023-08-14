namespace PixiEditor.AvaloniaUI.Models.Handlers.Tools;

internal interface IMoveToolHandler : IToolHandler
{
    public bool MoveAllLayers { get; }
    public bool KeepOriginalImage { get; }
    public bool TransformingSelectedArea { get; set; }
}
