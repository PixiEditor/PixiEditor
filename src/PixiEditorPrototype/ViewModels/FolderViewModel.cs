using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditorPrototype.Models;
using System.Collections.ObjectModel;

namespace PixiEditorPrototype.ViewModels
{
    internal class FolderViewModel : StructureMemberViewModel
    {
        public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
        public FolderViewModel(DocumentViewModel doc, DocumentHelpers helpers, IReadOnlyFolder member) : base(doc, helpers, member)
        {
        }


    }
}
