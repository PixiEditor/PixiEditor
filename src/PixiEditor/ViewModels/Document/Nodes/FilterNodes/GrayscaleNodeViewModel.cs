using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.FilterNodes;

internal class GrayscaleNodeViewModel : NodeViewModel<GrayscaleNode>
{
    public override LocalizedString DisplayName => "GRAYSCALE_FILTER_NODE";
    
    public override LocalizedString Category => "FILTERS";
}
