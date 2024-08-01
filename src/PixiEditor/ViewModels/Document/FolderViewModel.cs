using System.Collections.ObjectModel;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;
#nullable enable
internal class FolderViewModel : StructureMemberViewModel, IFolderHandler
{
    public FolderViewModel()
    {
        
    }
    
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid id) : base(doc, internals, id) { }
}
