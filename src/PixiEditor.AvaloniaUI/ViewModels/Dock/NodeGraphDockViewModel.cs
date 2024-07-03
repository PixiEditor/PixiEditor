using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class NodeGraphDockViewModel : DockableViewModel
{
    public const string TabId = "NodeGraph";
    
    public override string Id { get; } = TabId;
    public override string Title => new LocalizedString("NODE_GRAPH_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;
}
