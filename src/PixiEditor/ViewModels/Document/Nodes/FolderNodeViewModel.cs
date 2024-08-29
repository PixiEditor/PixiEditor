using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("FOLDER_NODE", "STRUCTURE")]
internal class FolderNodeViewModel : StructureMemberViewModel<FolderNode>, IFolderHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();
}
