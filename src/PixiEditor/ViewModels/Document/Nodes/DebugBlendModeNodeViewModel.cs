using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

internal class DebugBlendModeNodeViewModel : NodeViewModel<DebugBlendModeNode>
{
    public override LocalizedString DisplayName { get; } = "Debug Blend Mode";

    public override LocalizedString Category { get; } = "DEBUG";
}
