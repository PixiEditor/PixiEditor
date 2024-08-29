using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class LerpColorNodeViewModel : NodeViewModel<LerpColorNode>
{
    public override LocalizedString DisplayName => "LERP_NODE";
    
    public override LocalizedString Category => "NUMBERS";
}
