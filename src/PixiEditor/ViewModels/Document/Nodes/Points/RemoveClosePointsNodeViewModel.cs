using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Points;

internal class RemoveClosePointsNodeViewModel : NodeViewModel<RemoveClosePointsNode>
{
    public override LocalizedString DisplayName => "REMOVE_CLOSE_POINTS";
    
    public override LocalizedString Category => "SHAPE";
}
