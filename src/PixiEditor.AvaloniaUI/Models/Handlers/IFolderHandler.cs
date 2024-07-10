using System.Collections.ObjectModel;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IFolderHandler : IStructureMemberHandler
{
    internal ObservableCollection<IStructureMemberHandler> Children { get; }
}
