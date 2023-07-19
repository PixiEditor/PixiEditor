using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixiEditor.Models.Containers;

internal interface IFolderHandler : IStructureMemberHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; }
}
