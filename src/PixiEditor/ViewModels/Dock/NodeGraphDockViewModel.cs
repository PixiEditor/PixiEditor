using Avalonia.Input;
using PixiDocks.Core.Docking.Events;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Dock;

internal class NodeGraphDockViewModel : DockableViewModel, IDockableSelectionEvents
{
    private DocumentManagerViewModel document;

    public const string TabId = "NodeGraph";

    public override string Id { get; } = TabId;
    public override string Title => new LocalizedString("NODE_GRAPH_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    public DocumentManagerViewModel DocumentManagerSubViewModel
    {
        get => document;
        set => SetProperty(ref document, value);
    }

    public NodeGraphDockViewModel(DocumentManagerViewModel document)
    {
        DocumentManagerSubViewModel = document;

        TabCustomizationSettings.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.Nodes);
    }

    void IDockableSelectionEvents.OnSelected()
    {
        DocumentManagerSubViewModel.Owner.ShortcutController.OverwriteContext(this.GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        DocumentManagerSubViewModel.Owner.ShortcutController.ClearContext(this.GetType());
    }
}
