using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Helpers;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class DocumentReferencePropertyViewModel : NodePropertyViewModel<DocumentReference>
{
    private string? originalFilePath;
    public string? OriginalFilePath
    {
        get => originalFilePath;
        set => SetProperty(ref originalFilePath, value);
    }

    public ICommand PickGraphFileCommand { get; }

    public DocumentReferencePropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
        PickGraphFileCommand = new AsyncRelayCommand(OnPickGraphFile);
        ValueChanged += OnValueChanged;
    }

    private void OnValueChanged(INodePropertyHandler property, NodePropertyValueChangedArgs args)
    {
        if (args.NewValue is DocumentReference docRef)
        {
            OriginalFilePath = docRef.OriginalFilePath;
        }
        else
        {
            OriginalFilePath = null;
        }
    }

    private async Task OnPickGraphFile()
    {
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Pixi).GetFormattedTypes(false);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = any.ToList() });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            var doc = Importer.ImportDocument(dialog[0].Path.LocalPath);
            doc.Operations.InvokeCustomAction(() =>
            {
                Value = new DocumentReference(doc.FullFilePath, doc.Id, doc.AccessInternalReadOnlyDocument());
                OriginalFilePath = doc.FullFilePath;
            });
        }
    }
}
