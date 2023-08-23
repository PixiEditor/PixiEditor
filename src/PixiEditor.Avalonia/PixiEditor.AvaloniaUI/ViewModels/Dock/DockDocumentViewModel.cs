using PixiEditor.AvaloniaUI.ViewModels.Document;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class DockDocumentViewModel : global::Dock.Model.Avalonia.Controls.Document
{
    public DockDocumentViewModel(DocumentViewModel documentViewModel)
    {
        DataContext = documentViewModel;
    }
}
