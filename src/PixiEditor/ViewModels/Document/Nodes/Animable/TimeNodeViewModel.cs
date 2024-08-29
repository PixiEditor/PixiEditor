using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes.Animable;

internal class TimeNodeViewModel : NodeViewModel<TimeNode>
{
    public override LocalizedString DisplayName => "TIME_NODE";
    
    public override LocalizedString Category => "ANIMATION";
}
