using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.Nodes;
#nullable enable
internal class FolderNodeViewModel : StructureMemberViewModel<FolderNode>, IFolderHandler
{
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();
    
    // Dependent on layer name
    public override LocalizedString DisplayName => "";

    public override LocalizedString Category => "STRUCTURE";
}
