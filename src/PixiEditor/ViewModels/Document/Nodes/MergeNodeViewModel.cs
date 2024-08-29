using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class MergeNodeViewModel : NodeViewModel<MergeNode>
{
    public override LocalizedString DisplayName => "MERGE_NODE";
    
    public override LocalizedString Category => "IMAGE";
}
