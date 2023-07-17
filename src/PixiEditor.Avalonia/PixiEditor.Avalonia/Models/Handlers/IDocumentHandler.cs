using System.Collections.Generic;
using PixiEditor.Extensions.Palettes;

namespace PixiEditor.Models.Containers;

internal interface IDocumentHandler
{
    public static IDocumentHandler? Instance { get; }
    public bool HasActiveDocument { get; }
    public IDocument? ActiveDocument { get; set; }
}
