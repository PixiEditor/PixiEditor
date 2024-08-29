using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class SeparateColorNodeViewModel : NodeViewModel<SeparateColorNode>
{
    public override LocalizedString DisplayName => "SEPARATE_COLOR_NODE";
    
    public override LocalizedString Category => "COLOR";
}
