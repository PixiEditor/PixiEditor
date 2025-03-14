using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.SubViewModels;

internal class LazyViewportWindowViewModel : SubViewModel<WindowViewModel>, IDockableContent, IDockableCloseEvents,
    IDockableSelectionEvents
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Title => Path.GetFileName(LazyDocument.OriginalPath ?? LazyDocument.Path);
    public bool CanFloat { get; } = true;
    public bool CanClose { get; } = true;
    public TabCustomizationSettings TabCustomizationSettings { get; } = new DocumentTabCustomizationSettings();

    public LazyDocumentViewModel LazyDocument { get; }

    public LazyViewportWindowViewModel(WindowViewModel owner, LazyDocumentViewModel lazyDoc) : base(owner)
    {
        LazyDocument = lazyDoc;
    }

    void IDockableSelectionEvents.OnSelected()
    {
        Owner.ActiveWindow = this;
        Owner.Owner.ShortcutController.OverwriteContext(this.GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        Owner.Owner.ShortcutController.ClearContext(GetType());
    }

    async Task<bool> IDockableCloseEvents.OnClose()
    {
        Owner.OnLazyViewportWindowCloseButtonPressed(this);
        return await Task.FromResult(true);
    }
}
