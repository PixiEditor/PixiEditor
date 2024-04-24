using Avalonia.Media;
using PixiDocks.Core.Docking;
using PixiEditor.AvaloniaUI.Helpers.Converters;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class LayersDockViewModel : DockableViewModel
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
        TabCustomizationSettings.Icon =
            ImagePathToBitmapConverter.TryLoadBitmapFromRelativePath("/Images/Dockables/Layers.png");
    }

    private void DocumentManager_ActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        ActiveDocument = e.NewDocument;
    }
}
