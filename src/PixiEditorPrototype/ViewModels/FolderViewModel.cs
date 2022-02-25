using ChangeableDocument.Changeables.Interfaces;
using System.Collections.ObjectModel;

namespace PixiEditorPrototype.ViewModels
{
    internal class FolderViewModel : StructureMemberViewModel
    {
        public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
        public FolderViewModel(DocumentViewModel doc, IReadOnlyFolder member) : base(doc, member)
        {
        }


    }
}
