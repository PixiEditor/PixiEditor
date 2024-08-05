using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ChunkyImageLib;
using Newtonsoft.Json.Linq;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Exceptions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Files;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserData;
using PixiEditor.Numerics;
using PixiEditor.OperatingSystem;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using PixiEditor.Views.Windows;

namespace PixiEditor.ViewModels.SubViewModels;

[Command.Group("PixiEditor.File", "FILE")]
internal class FileViewModel : SubViewModel<ViewModelMain>
{
    private bool hasRecent;

    public bool HasRecent
    {
        get => hasRecent;
        set
        {
            hasRecent = value;
            OnPropertyChanged(nameof(HasRecent));
        }
    }

    public RecentlyOpenedCollection RecentlyOpened { get; init; }

    public FileViewModel(ViewModelMain owner)
        : base(owner)
    {
        Owner.OnStartupEvent += Owner_OnStartupEvent;
        RecentlyOpened = new RecentlyOpenedCollection(GetRecentlyOpenedDocuments());

        if (RecentlyOpened.Count > 0)
        {
            HasRecent = true;
        }

        PixiEditorSettings.File.MaxOpenedRecently.ValueChanged += (_, value) => UpdateMaxRecentlyOpened(value);
    }

    public void AddRecentlyOpened(string path)
    {
        if (RecentlyOpened.Contains(path))
        {
            RecentlyOpened.Move(RecentlyOpened.IndexOf(path), 0);
        }
        else
        {
            RecentlyOpened.Insert(0, path);
        }

        int maxCount = PixiEditorSettings.File.MaxOpenedRecently.Value;

        while (RecentlyOpened.Count > maxCount)
        {
            RecentlyOpened.RemoveAt(RecentlyOpened.Count - 1);
        }

        PixiEditorSettings.File.RecentlyOpened.Value = RecentlyOpened.Select(x => x.FilePath);
    }

    [Command.Internal("PixiEditor.File.RemoveRecent")]
    public void RemoveRecentlyOpened(string path)
    {
        if (!RecentlyOpened.Contains(path))
        {
            return;
        }

        RecentlyOpened.Remove(path);
        PixiEditorSettings.File.RecentlyOpened.Value = RecentlyOpened.Select(x => x.FilePath);
    }

    private void OpenHelloTherePopup()
    {
        new HelloTherePopup(this).Show();
    }

    private void Owner_OnStartupEvent(object sender, System.EventArgs e)
    {
        List<string> args = StartupArgs.Args;
        string file = args.FirstOrDefault(x => Importer.IsSupportedFile(x) && File.Exists(x));
        if (file != null)
        {
            OpenFromPath(file);
        }
        else if ((Owner.DocumentManagerSubViewModel.Documents.Count == 0 && !args.Contains("--crash")) &&
                 !args.Contains("--openedInExisting"))
        {
            if (PixiEditorSettings.StartupWindow.ShowStartupWindow.Value)
            {
                OpenHelloTherePopup();
            }
        }
    }

    [Command.Internal("PixiEditor.File.OpenRecent")]
    public void OpenRecent(string parameter)
    {
        string path = parameter;
        if (!File.Exists(path))
        {
            NoticeDialog.Show("FILE_NOT_FOUND", "FAILED_TO_OPEN_FILE");
            RecentlyOpened.Remove(path);
            PixiEditorSettings.File.RecentlyOpened.Value = RecentlyOpened.Select(x => x.FilePath);
            return;
        }

        OpenFromPath(path);
    }

    [Command.Basic("PixiEditor.File.Open", "OPEN", "OPEN_FILE", Key = Key.O, Modifiers = KeyModifiers.Control,
        MenuItemPath = "FILE/OPEN_FILE", MenuItemOrder = 1, Icon = PixiPerfectIcons.FileText)]
    public async Task OpenFromOpenFileDialog()
    {
        var filter = SupportedFilesHelper.BuildOpenFilter();

        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dialog = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions { FileTypeFilter = filter });

            if (dialog.Count == 0 || !Importer.IsSupportedFile(dialog[0].Path.LocalPath))
                return;

            OpenFromPath(dialog[0].Path.LocalPath);
        }
    }

    [Command.Basic("PixiEditor.File.OpenFileFromClipboard", "OPEN_FILE_FROM_CLIPBOARD",
        "OPEN_FILE_FROM_CLIPBOARD_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.HasImageInClipboard")]
    public async Task OpenFromClipboard()
    {
        var images = await ClipboardController.GetImagesFromClipboard();

        foreach (var dataImage in images)
        {
            if (File.Exists(dataImage.name))
            {
                OpenRegularImage(dataImage.image, null);
                continue;
            }

            OpenRegularImage(dataImage.image, null);
        }
    }

    private bool MakeExistingDocumentActiveIfOpened(string path)
    {
        foreach (DocumentViewModel document in Owner.DocumentManagerSubViewModel.Documents)
        {
            if (document.FullFilePath is not null &&
                System.IO.Path.GetFullPath(document.FullFilePath) == System.IO.Path.GetFullPath(path))
            {
                Owner.WindowSubViewModel.MakeDocumentViewportActive(document);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to open the passed file if it isn't already open
    /// </summary>
    public void OpenFromPath(string path, bool associatePath = true)
    {
        if (MakeExistingDocumentActiveIfOpened(path))
            return;

        try
        {
            if (path.EndsWith(".pixi"))
            {
                OpenDotPixi(path, associatePath);
            }
            else
            {
                OpenRegularImage(path, associatePath);
            }
        }
        catch (RecoverableException ex)
        {
            NoticeDialog.Show(ex.DisplayMessage, "ERROR");
        }
        catch (OldFileFormatException)
        {
            NoticeDialog.Show("OLD_FILE_FORMAT_DESCRIPTION", "OLD_FILE_FORMAT");
        }
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    private void OpenDotPixi(string path, bool associatePath = true)
    {
        DocumentViewModel document = Importer.ImportDocument(path, associatePath);

        AddDocumentViewModelToTheSystem(document);
        AddRecentlyOpened(document.FullFilePath);
        
        var fileSize = new FileInfo(path).Length;
        Analytics.SendOpenFile(PixiFileType.PixiFile, fileSize, document.SizeBindable);
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    public void OpenRecoveredDotPixi(string? originalPath, byte[] dotPixiBytes)
    {
        DocumentViewModel document = Importer.ImportDocument(dotPixiBytes, originalPath);
        document.MarkAsUnsaved();
        AddDocumentViewModelToTheSystem(document);
    }

    /// <summary>
    /// Opens a regular image file from path, creates a document from it, and adds it to the system.
    /// </summary>
    private void OpenRegularImage(string path, bool associatePath)
    {
        var image = Importer.ImportImage(path, VecI.NegativeOne);

        if (image == null) return;

        var doc = NewDocument(b => b
            .WithSize(image.Size)
            .WithGraph(x => x
                .WithImageLayerNode(
                    new LocalizedString("IMAGE"),
                    image, out int id)
                .WithOutputNode(id, "Output")
            ));

        if (associatePath)
        {
            doc.FullFilePath = path;
        }

        AddRecentlyOpened(path);

        var fileType = SupportedFilesHelper.ParseImageFormat(path);

        if (fileType != null)
        {
            var fileSize = new FileInfo(path).Length;
            Analytics.SendOpenFile(fileType, fileSize, doc.SizeBindable);
        }
        else
        {
            CrashHelper.SendExceptionInfoToWebhook(new InvalidFileTypeException(default, $"Invalid file type '{fileType}'"));
        }
    }


    /// <summary>
    /// Opens a regular image file from path, creates a document from it, and adds it to the system.
    /// </summary>
    private void OpenRegularImage(Surface surface, string path)
    {
        DocumentViewModel doc = NewDocument(b => b
            .WithSize(surface.Size)
            .WithGraph(x => x
                .WithImageLayerNode(
                    new LocalizedString("IMAGE"),
                    surface, out int id)
                .WithOutputNode(id, "Output")
            ));

        if (path == null)
        {
            return;
        }

        doc.FullFilePath = path;
        AddRecentlyOpened(path);
    }

    [Command.Basic("PixiEditor.File.New", "NEW_IMAGE", "CREATE_NEW_IMAGE", Key = Key.N,
        Modifiers = KeyModifiers.Control,
        MenuItemPath = "FILE/NEW_FILE", MenuItemOrder = 0, Icon = PixiPerfectIcons.File)]
    public async Task CreateFromNewFileDialog()
    {
        Window mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            .MainWindow;
        NewFileDialog newFile = new NewFileDialog(mainWindow);
        if (await newFile.ShowDialog())
        {
            NewDocument(b => b
                .WithSize(newFile.Width, newFile.Height)
                .WithGraph(x => x
                    .WithImageLayerNode(
                        new LocalizedString("BASE_LAYER_NAME"),
                        new VecI(newFile.Width, newFile.Height), out int id)
                    .WithOutputNode(id, "Output")
                ));

            Analytics.SendCreateDocument(newFile.Width, newFile.Height);
        }
    }

    public DocumentViewModel NewDocument(Action<DocumentViewModelBuilder> builder)
    {
        var doc = DocumentViewModel.Build(builder);
        AddDocumentViewModelToTheSystem(doc);
        return doc;
    }

    private void AddDocumentViewModelToTheSystem(DocumentViewModel doc)
    {
        Owner.DocumentManagerSubViewModel.Documents.Add(doc);
        Owner.WindowSubViewModel.CreateNewViewport(doc);
        Owner.WindowSubViewModel.MakeDocumentViewportActive(doc);
    }

    [Command.Basic("PixiEditor.File.Save", false, "SAVE", "SAVE_IMAGE", CanExecute = "PixiEditor.HasDocument",
        Key = Key.S, Modifiers = KeyModifiers.Control, Icon = PixiPerfectIcons.Save,
        MenuItemPath = "FILE/SAVE_PIXI", MenuItemOrder = 3)]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "SAVE_AS", "SAVE_IMAGE_AS", CanExecute = "PixiEditor.HasDocument",
        Key = Key.S, Modifiers = KeyModifiers.Control | KeyModifiers.Shift, Icon = PixiPerfectIcons.Save,
        MenuItemPath = "FILE/SAVE_AS_PIXI", MenuItemOrder = 4)]
    public async Task<bool> SaveActiveDocument(bool asNew)
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return await SaveDocument(doc, asNew);
    }

    public async Task<bool> SaveDocument(DocumentViewModel document, bool asNew)
    {
        string finalPath = null;
        if (asNew || string.IsNullOrEmpty(document.FullFilePath))
        {
            var result = await Exporter.TrySaveWithDialog(document, ExportConfig.Empty);
            if (result.Result == DialogSaveResult.Cancelled)
                return false;
            if (result.Result != DialogSaveResult.Success)
            {
                ShowSaveError(result.Result);
                return false;
            }

            finalPath = result.Path;
            AddRecentlyOpened(result.Path);
        }
        else
        {
            var result = await Exporter.TrySaveAsync(document, document.FullFilePath, ExportConfig.Empty);
            if (result != SaveResult.Success)
            {
                ShowSaveError((DialogSaveResult)result);
                return false;
            }

            finalPath = document.FullFilePath;
        }

        document.FullFilePath = finalPath;
        document.MarkAsSaved();
        return true;
    }

    /// <summary>
    ///     Generates export dialog or saves directly if save data is known.
    /// </summary>
    /// <param name="parameter">CommandProperty.</param>
    [Command.Basic("PixiEditor.File.Export", "EXPORT", "EXPORT_IMAGE", CanExecute = "PixiEditor.HasDocument",
        Key = Key.E, Modifiers = KeyModifiers.Control,
        MenuItemPath = "FILE/EXPORT_IMG", MenuItemOrder = 5, Icon = PixiPerfectIcons.Image)]
    public async Task ExportFile()
    {
        try
        {
            DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
            if (doc is null)
                return;

            ExportFileDialog info = new ExportFileDialog(MainWindow.Current, doc.SizeBindable, doc)
            {
                SuggestedName = Path.GetFileNameWithoutExtension(doc.FileName)
            };
            if (await info.ShowDialog())
            {
                var result =
                    await Exporter.TrySaveUsingDataFromDialog(doc, info.FilePath, info.ChosenFormat, info.ExportConfig);
                if (result.result == SaveResult.Success)
                    IOperatingSystem.Current.OpenFolder(result.finalPath);
                else
                    ShowSaveError((DialogSaveResult)result.result);
            }
        }
        catch (RecoverableException e)
        {
            NoticeDialog.Show(e.DisplayMessage, "ERROR");
        }
    }

    private void ShowSaveError(DialogSaveResult result)
    {
        switch (result)
        {
            case DialogSaveResult.InvalidPath:
                NoticeDialog.Show("ERROR", "ERROR_SAVE_LOCATION");
                break;
            case DialogSaveResult.ConcurrencyError:
                NoticeDialog.Show("INTERNAL_ERROR", "ERROR_WHILE_SAVING");
                break;
            case DialogSaveResult.SecurityError:
                NoticeDialog.Show(title: "SECURITY_ERROR", message: "SECURITY_ERROR_MSG");
                break;
            case DialogSaveResult.IoError:
                NoticeDialog.Show(title: "IO_ERROR", message: "IO_ERROR_MSG");
                break;
            case DialogSaveResult.UnknownError:
                NoticeDialog.Show("ERROR", "UNKNOWN_ERROR_SAVING");
                break;
        }
    }

    private void UpdateMaxRecentlyOpened(int newAmount)
    {
        if (newAmount >= RecentlyOpened.Count)
        {
            return;
        }

        List<RecentlyOpenedDocument> recentlyOpenedDocuments =
            new List<RecentlyOpenedDocument>(RecentlyOpened.Take(newAmount));

        RecentlyOpened.Clear();

        foreach (RecentlyOpenedDocument recent in recentlyOpenedDocuments)
        {
            RecentlyOpened.Add(recent);
        }
    }

    private List<RecentlyOpenedDocument> GetRecentlyOpenedDocuments()
    {
        var paths = PixiEditorSettings.File.RecentlyOpened.Value.Take(PixiEditorSettings.File.MaxOpenedRecently.Value);
        List<RecentlyOpenedDocument> documents = new List<RecentlyOpenedDocument>();

        foreach (string path in paths)
        {
            if (!File.Exists(path))
                continue;
            documents.Add(new RecentlyOpenedDocument(path));
        }

        return documents;
    }
}
