namespace PixiEditor.Models.Handlers.Tools;

internal interface IRasterLineToolHandler : IToolHandler
{
    public int ToolSize { get; }
    public bool Snap { get; }
}
