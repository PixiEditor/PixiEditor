using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class EllipseNodeViewModel : NodeViewModel<EllipseNode>
{
    public override LocalizedString DisplayName => "ELLIPSE_NODE";
    
    public override LocalizedString Category => "SHAPE";
}
