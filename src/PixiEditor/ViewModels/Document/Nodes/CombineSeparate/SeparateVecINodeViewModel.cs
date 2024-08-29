using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class SeparateVecINodeViewModel : NodeViewModel<SeparateVecINode>
{
    public override LocalizedString DisplayName => "SEPARATE_VECI_NODE";
    
    public override LocalizedString Category => "NUMBERS";
}
