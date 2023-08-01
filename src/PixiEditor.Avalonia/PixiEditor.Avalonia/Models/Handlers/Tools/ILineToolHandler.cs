namespace PixiEditor.Models.Containers.Tools;

internal interface ILineToolHandler : IToolHandler
{
    public int ToolSize { get; }
    public bool Snap { get; }
}
