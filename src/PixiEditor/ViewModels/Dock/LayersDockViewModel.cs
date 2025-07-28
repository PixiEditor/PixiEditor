using Avalonia.Media;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.Helpers.Converters;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.ViewModels.Dock;

internal class LayersDockViewModel : DockableViewModel, IDockableSelectionEvents
{
    public const string TabId = "Layers";
    public override string Id => TabId;
    public override string Title => new LocalizedString("LAYERS_TITLE");
    public override bool CanFloat => true;
    public override bool CanClose => true;

    private DocumentManagerViewModel documentManager;
    private DocumentViewModel activeDocument;

    public DocumentManagerViewModel DocumentManager
    {
        get => documentManager;
        set => SetProperty(ref documentManager, value);
    }

    public DocumentViewModel ActiveDocument
    {
        get => activeDocument;
        set => SetProperty(ref activeDocument, value);
    }

    public LayersDockViewModel(DocumentManagerViewModel documentManager)
    {
        DocumentManager = documentManager;
        DocumentManager.ActiveDocumentChanged += DocumentManager_ActiveDocumentChanged;
        TabCustomizationSettings.Icon = PixiPerfectIconExtensions.ToIcon(PixiPerfectIcons.Layers);
    }

    private void DocumentManager_ActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        ActiveDocument = e.NewDocument;
    }

    void IDockableSelectionEvents.OnSelected()
    {
        documentManager.Owner.ShortcutController.OverwriteContext(GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        documentManager.Owner.ShortcutController.ClearContext(GetType());
    }
}
