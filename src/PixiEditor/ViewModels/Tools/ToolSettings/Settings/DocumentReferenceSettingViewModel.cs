using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal sealed class DocumentReferenceSettingViewModel : Setting<DocumentReference>
{
    public AsyncRelayCommand<DocumentReference> PickDocumentCommand { get; }
    public bool HasOriginalFilePath => !string.IsNullOrEmpty(Value.OriginalFilePath);

    public DocumentReferenceSettingViewModel(string name) : base(name)
    {
        PickDocumentCommand = new AsyncRelayCommand<DocumentReference>(PickDocument);
    }

    private async Task PickDocument(DocumentReference? arg)
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
                OnPropertyChanged(nameof(HasOriginalFilePath));
            });
        }
    }
}
