using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Points;

internal class DistributePointsNodeViewModel : NodeViewModel<DistributePointsNode>
{
    public override LocalizedString DisplayName => "DISTRIBUTE_POINTS";
    
    public override LocalizedString Category => "SHAPE";
}
