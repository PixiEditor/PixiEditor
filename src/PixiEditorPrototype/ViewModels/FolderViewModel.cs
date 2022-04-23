using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditorPrototype.Models;

namespace PixiEditorPrototype.ViewModels;

internal class FolderViewModel : StructureMemberViewModel
{
    public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyFolder member) : base(doc, helpers, member)
    {
    }


}
