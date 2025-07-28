using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("FOLDER_NODE", "STRUCTURE", PixiPerfectIcons.Folder)]
internal class FolderNodeViewModel : StructureMemberViewModel<FolderNode>, IFolderHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();

    public int CountChildrenRecursive()
    {
        int count = 0;
        foreach (var child in Children)
        {
            if (child is FolderNodeViewModel folder)
            {
                count += folder.CountChildrenRecursive();
            }
            else
            {
                count++;
            }
        }

        return count;
    }
}
