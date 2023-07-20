namespace PixiEditor.Models.Containers.Tools;

internal interface IMoveToolHandler : IToolHandler
{
    public bool MoveAllLayers { get; set; }
    public bool KeepOriginalImage { get; set; }
    public bool TransformingSelectedArea { get; set; }
}
