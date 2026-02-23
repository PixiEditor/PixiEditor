using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("NESTED_DOCUMENT", "STRUCTURE", PixiPerfectIcons.File)]
internal partial class NestedDocumentNodeViewModel :
    StructureMemberViewModel<ChangeableDocument.Changeables.Graph.Nodes.NestedDocumentNode>, ILayerHandler
{
    bool lockTransparency;
    Guid? referenceId = null;
    string? _linkedDocumentPath = null;

    public Matrix3X3 TransformationMatrix => (Internals.Tracker.Document.FindMember(Id) as ITransformableObject)
        ?.TransformationMatrix ?? Matrix3X3.Identity;

    public void SetLockTransparency(bool lockTransparency)
    {
        this.lockTransparency = lockTransparency;
        OnPropertyChanged(nameof(LockTransparencyBindable));
    }

    public bool LockTransparencyBindable
    {
        get => lockTransparency;
        set
        {
            if (!Document.BlockingUpdateableChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new LayerLockTransparency_Action(Id, value));
        }
    }

    private bool shouldDrawOnMask = false;

    public bool ShouldDrawOnMask
    {
        get => shouldDrawOnMask;
        set
        {
            if (value == shouldDrawOnMask)
                return;
            shouldDrawOnMask = value;
            OnPropertyChanged(nameof(ShouldDrawOnMask));
        }
    }

    public bool IsLinkHealthy => IsFileLinked ? LinkedPathExists() : referenceId != null && referenceId != Guid.Empty;
    public bool IsFileLinked => !string.IsNullOrEmpty(_linkedDocumentPath);

    public Type? QuickEditTool => typeof(MoveToolViewModel);

    public bool IsLinked => _linkedDocumentPath != null || (referenceId != null && referenceId != Guid.Empty);
    public string? LinkedDocumentPath => LinkedPathExists() ? _linkedDocumentPath : referenceId?.ToString();
    public Guid ReferenceId => referenceId ?? Guid.Empty;
    public string FilePath => _linkedDocumentPath ?? string.Empty;

    [RelayCommand]
    public async Task UnlinkWithDialog()
    {
        var result = await ConfirmationDialog.Show("UNLINK_DOCUMENT_MESSAGE", "UNLINK_DOCUMENT_TITLE");
        if (result == ConfirmationType.Yes)
        {
            Internals.ActionAccumulator.AddFinishedActions(new UnlinkNestedDocument_Action(Id));
        }
    }

    [RelayCommand]
    public async Task RelinkWithDialog()
    {
        var filter = SupportedFilesHelper.BuildOpenFilter();

        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                string fullSelectedPath;
                if (string.IsNullOrEmpty(_linkedDocumentPath))
                {
                    fullSelectedPath = await RelinkFile(desktop);
                }
                else
                {
                    fullSelectedPath = await RelinkFolder(desktop);
                }

                Internals.ActionAccumulator.AddFinishedActions(new ChangeDocumentReferenceFilePath_Action(Id, fullSelectedPath), new InvokeAction_PassthroughAction(() =>
                {
                    ViewModelMain.Current.DocumentManagerSubViewModel.ReloadDocumentReference(Id, fullSelectedPath);
                }));
            }
        }
        catch (Exception e)
        {
            NoticeDialog.Show(new LocalizedString("ERROR_RELINKING_DOCUMENT", e.Message), "ERROR");
        }
    }

    private async Task<string> RelinkFolder(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var suggestedPath =
            await desktop.MainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        string directory = Path.GetDirectoryName(_linkedDocumentPath) ?? string.Empty;

        if (IsFileLinked && !string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            suggestedPath = await desktop.MainWindow.StorageProvider.TryGetFolderFromPathAsync(directory);

        var dialog = await desktop.MainWindow.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = suggestedPath,
                Title = new LocalizedString("RELINK_DOCUMENT_TITLE")
            });

        if (dialog.Count == 0)
            return null;

        string selectedPath = dialog[0].Path.LocalPath;
        string fullSelectedPath = Path.Combine(selectedPath, Path.GetFileName(_linkedDocumentPath ?? string.Empty));
        if (!SupportedFilesHelper.IsSupported(fullSelectedPath))
        {
            throw new InvalidFileTypeException(new LocalizedString("FILE_EXTENSION_NOT_SUPPORTED",
                Path.GetExtension(fullSelectedPath)));
        }

        return fullSelectedPath;
    }

    private async Task<string> RelinkFile(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var suggestedPath =
            await desktop.MainWindow.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);

        var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                SuggestedStartLocation = suggestedPath,
                Title = new LocalizedString("RELINK_DOCUMENT_TITLE"),
                FileTypeFilter = SupportedFilesHelper.BuildOpenFilter()
            });

        if (dialog.Count == 0)
            return null;

        string fullSelectedPath = dialog[0].Path.LocalPath;
        return fullSelectedPath;
    }

    public void SetOriginalFilePath(string? infoOriginalFilePath)
    {
        _linkedDocumentPath = infoOriginalFilePath;
        OnPropertyChanged(nameof(IsLinked));
        OnPropertyChanged(nameof(LinkedDocumentPath));
        OnPropertyChanged(nameof(IsLinkHealthy));
        OnPropertyChanged(nameof(IsFileLinked));
    }

    public void SetReferenceId(Guid? infoReferenceId)
    {
        referenceId = infoReferenceId;
        OnPropertyChanged(nameof(IsLinked));
        OnPropertyChanged(nameof(LinkedDocumentPath));
        OnPropertyChanged(nameof(IsLinkHealthy));
        OnPropertyChanged(nameof(IsFileLinked));
    }

    private bool LinkedPathExists()
    {
        return IsFileLinked && System.IO.File.Exists(_linkedDocumentPath);
    }

    public void UpdateLinkedStatus()
    {
        OnPropertyChanged(nameof(IsLinked));
        OnPropertyChanged(nameof(LinkedDocumentPath));
        OnPropertyChanged(nameof(IsLinkHealthy));
        OnPropertyChanged(nameof(IsFileLinked));
    }
}
