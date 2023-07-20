namespace PixiEditor.Models.Containers.Tools;

internal interface ILineToolHandler : IToolHandler
{
    public int ToolSize { get; set; }
    public bool Snap { get; set; }
}
