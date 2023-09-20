using Avalonia;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class LayersDockViewModel : Tool
{
    public static readonly StyledProperty<DocumentManagerViewModel> DocumentManagerProperty = AvaloniaProperty.Register<LayersDockViewModel, DocumentManagerViewModel>(
        nameof(DocumentManager));

    public DocumentManagerViewModel DocumentManager
    {
        get => GetValue(DocumentManagerProperty);
        set => SetValue(DocumentManagerProperty, value);
    }

    public static readonly StyledProperty<DocumentViewModel> ActiveDocumentProperty = AvaloniaProperty.Register<LayersDockViewModel, DocumentViewModel>(
        nameof(ActiveDocument));

    public DocumentViewModel ActiveDocument
    {
        get => GetValue(ActiveDocumentProperty);
        set => SetValue(ActiveDocumentProperty, value);
    }
    
    public LayersDockViewModel(DocumentManagerViewModel documentManager)
    {
        DocumentManager = documentManager;
        DocumentManager.ActiveDocumentChanged += DocumentManager_ActiveDocumentChanged;
    }

    private void DocumentManager_ActiveDocumentChanged(object? sender, DocumentChangedEventArgs e)
    {
        ActiveDocument = e.NewDocument;
    }
}
