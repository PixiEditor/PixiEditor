namespace PixiEditor.Models.Handlers.Tools;

internal interface ILineToolHandler : IToolHandler
{
    public double ToolSize { get; }
    public bool Snap { get; }
    public bool DrawFromCenter { get; }
}
