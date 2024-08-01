using Avalonia.Input;
using PixiDocks.Core.Docking.Events;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Dock;

internal class NodeGraphDockViewModel(DocumentManagerViewModel document) : DockableViewModel, IDockableSelectionEvents
{
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
    
    void IDockableSelectionEvents.OnSelected()
    {
        DocumentManagerSubViewModel.Owner.ShortcutController.OverwriteContext(this.GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        DocumentManagerSubViewModel.Owner.ShortcutController.ClearContext(this.GetType());
    }
}
