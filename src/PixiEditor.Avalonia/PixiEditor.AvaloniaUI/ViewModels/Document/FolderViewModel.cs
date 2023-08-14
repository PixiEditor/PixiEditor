using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;
#nullable enable
internal class FolderViewModel : StructureMemberViewModel, IFolderHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();
    public FolderViewModel(DocumentViewModel doc, DocumentInternalParts internals, Guid guidValue) : base(doc, internals, guidValue) { }
}
