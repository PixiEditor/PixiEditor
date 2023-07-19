using System.Collections.Generic;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Models.Containers;

internal interface IDocumentManagerHandler
{
    public static IDocumentManagerHandler? Instance { get; }
    public bool HasActiveDocument { get; }
    public IDocument? ActiveDocument { get; set; }
}
