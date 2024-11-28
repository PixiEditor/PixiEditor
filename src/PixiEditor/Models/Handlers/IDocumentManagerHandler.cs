namespace PixiEditor.Models.Handlers;

internal interface IDocumentManagerHandler : IHandler
{
    public static IDocumentManagerHandler? Instance { get; }
    public bool HasActiveDocument { get; }
    public IDocument? ActiveDocument { get; set; }
}
