using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.CombineSeparate;

internal class CombineColorNodeViewModel : NodeViewModel<CombineColorNode>
{
    public override LocalizedString DisplayName => "COMBINE_COLOR_NODE";
    
    public override LocalizedString Category => "COLOR";
}
