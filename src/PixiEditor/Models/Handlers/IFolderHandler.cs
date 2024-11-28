using System.Collections.ObjectModel;

namespace PixiEditor.Models.Handlers;

internal interface IFolderHandler : IStructureMemberHandler
{
    internal ObservableCollection<IStructureMemberHandler> Children { get; }
}
