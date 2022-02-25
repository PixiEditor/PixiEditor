using ChangeableDocument.Changeables.Interfaces;

namespace PixiEditorPrototype.ViewModels
{
    internal class LayerViewModel : StructureMemberViewModel
    {
        public LayerViewModel(DocumentViewModel doc, IReadOnlyLayer layer) : base(doc, layer)
        {
        }
    }
}
