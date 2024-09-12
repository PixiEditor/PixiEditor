namespace PixiEditor.Models.Handlers.Tools;

internal interface IVectorLineToolHandler : ILineToolHandler 
{
    public int ToolSize { get; }
    public bool Snap { get; }
}
