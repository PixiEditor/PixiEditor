using System.Collections.ObjectModel;
using PixiEditor.Models.DocumentModels;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
internal class FolderViewModel : StructureMemberViewModel
{
    public ObservableCollection<StructureMemberViewModel> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentHelpers helpers, Guid guidValue) : base(doc, helpers, guidValue) { }
}
