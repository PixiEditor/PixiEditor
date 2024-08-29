using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

internal class ColorMatrixFilterNodeViewModel : NodeViewModel<ColorMatrixFilterNode>
{
    public override LocalizedString DisplayName => "COLOR_MATRIX_TRANSFORM_FILTER_NODE";
    
    public override LocalizedString Category => "FILTERS";
}
