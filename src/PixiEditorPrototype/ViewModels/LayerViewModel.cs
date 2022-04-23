using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.ViewModels;

internal class LayerViewModel : StructureMemberViewModel
{
    public LayerViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyLayer layer) : base(doc, helpers, layer)
    {
    }
}
