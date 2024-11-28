namespace PixiEditor.Models.Handlers.Tools;

internal interface IFloodFillToolHandler : IToolHandler
{
    public bool ConsiderAllLayers { get; }
    public float Tolerance { get; }
}
