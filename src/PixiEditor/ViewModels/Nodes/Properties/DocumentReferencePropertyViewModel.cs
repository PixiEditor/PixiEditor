using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class DocumentReferencePropertyViewModel : NodePropertyViewModel<DocumentReference>
{
    private string? originalFilePath;
    public string? OriginalFilePath
    {
        get => originalFilePath;
        set
        {
            if (SetProperty(ref originalFilePath, value))
            {
                OnPropertyChanged(nameof(HasOriginalPath));
            }
        }
    }

    public ICommand PickGraphFileCommand { get; }
    public bool HasOriginalPath => !string.IsNullOrEmpty(OriginalFilePath);

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
        var any = new FileTypeDialogDataSet(FileTypeDialogDataSet.SetKind.Any).GetFormattedTypes(true);

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = any.ToList() });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            var doc = FileViewModel.ImportFromPath(dialog[0].Path.LocalPath);
            doc.Operations.InvokeCustomAction(() =>
            {
                Value = new DocumentReference(doc.FullFilePath, doc.Id, doc.AccessInternalReadOnlyDocument());
                OriginalFilePath = doc.FullFilePath;
            });
        }
    }
}
