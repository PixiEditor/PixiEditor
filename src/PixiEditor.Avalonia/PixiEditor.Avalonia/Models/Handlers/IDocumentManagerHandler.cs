using System.Collections.Generic;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Models.Containers;

internal interface IDocumentManagerHandler : IHandler
{
    public static IDocumentManagerHandler? Instance { get; }
    public bool HasActiveDocument { get; }
    public IDocument? ActiveDocument { get; set; }
}
