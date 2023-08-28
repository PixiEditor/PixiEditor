using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Avalonia.Controls;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class PixiEditorDocumentDock : DocumentDock
{
    private FileViewModel manager;
    public PixiEditorDocumentDock(FileViewModel manager)
    {
        this.manager = manager;
        manager.Owner.WindowSubViewModel.Viewports.CollectionChanged += Viewports_CollectionChanged;
        CreateDocument = new RelayCommand(CreateDockDocument);
    }

    private void Viewports_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ViewportWindowViewModel viewportVm = e.NewItems[^1] as ViewportWindowViewModel;
        var document = new DockDocumentViewModel(viewportVm) { Title = viewportVm.Document.FileName };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }

    private void CreateDockDocument()
    {
        if (!CanCreateDocument)
        {
            return;
        }

        var doc = manager.NewDocument(b =>
        {
            b.WithSize(new VecI(64, 64));
        });
        var document = new DockDocumentViewModel(manager.Owner.WindowSubViewModel.Viewports[^1]) { Title = "hello" };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
