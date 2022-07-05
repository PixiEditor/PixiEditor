using System.Collections.ObjectModel;
using PixiEditor.Models.DocumentModels;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels.Prototype;

internal class FolderViewModel : StructureMemberViewModel
{
    public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue) : base(doc, helpers, guidValue) { }
}
