using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class SeparateVecDNodeViewModel : NodeViewModel<SeparateVecDNode>
{
    public override LocalizedString DisplayName => "SEPARATE_VECD_NODE";
    
    public override LocalizedString Category => "NUMBERS";
}
