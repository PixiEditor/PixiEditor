namespace PixiEditor.Models.Containers.Tools;

internal interface IFloodFillToolHandler : IToolHandler
{
    public bool ConsiderAllLayers { get; set; }
}
