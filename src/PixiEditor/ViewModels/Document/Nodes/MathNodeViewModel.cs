using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class MathNodeViewModel : NodeViewModel<MathNode>
{
    public override LocalizedString DisplayName => "MATH_NODE";
    
    public override LocalizedString Category => "NUMBERS";
}
