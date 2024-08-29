using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class CombineVecINodeViewModel : NodeViewModel<CombineVecINode>
{
    public override LocalizedString DisplayName => "COMBINE_VECI_NODE";
    
    public override LocalizedString Category => "NUMBERS";
}
