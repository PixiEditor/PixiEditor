using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.Nodes;
#nullable enable
internal class FolderViewModel : StructureMemberViewModel<FolderNode>, IFolderHandler
{
    public FolderViewModel()
    {
        
    }
    
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid id) : base(doc, internals, id) { }
}
