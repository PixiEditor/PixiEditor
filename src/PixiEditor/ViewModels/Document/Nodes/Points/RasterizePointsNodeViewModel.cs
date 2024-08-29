using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Points;

internal class RasterizePointsNodeViewModel : NodeViewModel<RasterizePointsNode>
{
    public override LocalizedString DisplayName => "RASTERIZE_POINTS";
    
    public override LocalizedString Category => "SHAPE";
}
