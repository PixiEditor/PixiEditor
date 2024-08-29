using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class OutputNodeViewModel : NodeViewModel<OutputNode>
{
    public override LocalizedString DisplayName => "OUTPUT_NODE";
    
    public override LocalizedString Category => "";
}
