using CommunityToolkit.Mvvm.Input;
using Dock.Model.Avalonia.Controls;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

public class PixiEditorDocumentDock : DocumentDock
{
    public PixiEditorDocumentDock()
    {
        CreateDocument = new RelayCommand(CreateDockDocument);
    }

    private void CreateDockDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var document = new DockDocumentViewModel() { Title = "hello" };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
