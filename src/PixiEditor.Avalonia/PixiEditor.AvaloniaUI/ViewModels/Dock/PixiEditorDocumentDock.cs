using System.Collections.Specialized;
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
        manager.Owner.DocumentManagerSubViewModel.Documents.CollectionChanged += Documents_CollectionChanged;
        CreateDocument = new RelayCommand(CreateDockDocument);
    }

    private void Documents_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        DocumentViewModel documentVm = e.NewItems[^1] as DocumentViewModel;
        var document = new DockDocumentViewModel(documentVm) { Title = documentVm.FileName };

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
        var document = new DockDocumentViewModel(doc) { Title = "hello" };

        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }
}
