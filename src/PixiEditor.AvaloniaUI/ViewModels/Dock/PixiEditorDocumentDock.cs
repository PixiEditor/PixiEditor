using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class PixiEditorDocumentDock : DocumentDock
{
    private FileViewModel manager;
    public PixiEditorDocumentDock(FileViewModel manager)
    {
        this.manager = manager;
        manager.Owner.WindowSubViewModel.ViewportAdded += AddNewViewport;
        manager.Owner.WindowSubViewModel.ViewportClosed += RemoveViewport;
        CreateDocument = new AsyncRelayCommand(CreateDockDocument);
    }

    private void AddNewViewport(ViewportWindowViewModel e)
    {
        var document = new DockDocumentViewModel(e) { Title = e.Document.FileName };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }

    private void RemoveViewport(ViewportWindowViewModel e)
    {
        var document = VisibleDockables?.OfType<DockDocumentViewModel>().FirstOrDefault(x => x.ViewModel == e);
        if (document != null)
        {
            Factory?.RemoveDockable(this, true);
        }
    }

    private async Task CreateDockDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        await manager.CreateFromNewFileDialog();
    }
}
