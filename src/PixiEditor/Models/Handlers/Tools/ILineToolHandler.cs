namespace PixiEditor.Models.Handlers.Tools;

internal interface ILineToolHandler : IToolHandler
{
    public int ToolSize { get; }
    public bool Snap { get; }
}
