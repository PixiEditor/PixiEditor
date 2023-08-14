using System.Collections.ObjectModel;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IFolderHandler : IStructureMemberHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; }
}
