using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
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
using Drawie.Numerics;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.IO.CustomDocumentFormats;
using PixiEditor.OperatingSystem;
using PixiEditor.Parser;
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
    public IReadOnlyList<IDocumentBuilder> DocumentBuilders => documentBuilders;

    private List<IDocumentBuilder> documentBuilders;

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
        documentBuilders = owner.Services.GetServices<IDocumentBuilder>().ToList();
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
        List<string> args = StartupArgs.Args;
        string file = args.FirstOrDefault(x => Importer.IsSupportedFile(x) && File.Exists(x));

        var preferences = IPreferences.Current;

        try
        {
            if (!args.Contains("--crash"))
            {
                var lastCrash = preferences!.GetLocalPreference<string>(PreferencesConstants.LastCrashFile);

                if (lastCrash == null)
                {
                    MaybeReopenTempAutosavedFiles();
                }
                else
                {
                    preferences.UpdateLocalPreference<string>(PreferencesConstants.LastCrashFile, null);

                    var report = CrashReport.Parse(lastCrash);
                    OpenFromReport(report, out bool showMissingFilesDialog);

                    if (showMissingFilesDialog)
                    {
                        CrashReportViewModel.ShowMissingFilesDialog(report);
                    }
                }
            }
        }
        catch (Exception exc)
        {
            CrashHelper.SendExceptionInfo(exc);
        }

        if (file != null)
        {
            OpenFromPath(file);
        }
        else if ((Owner.DocumentManagerSubViewModel.Documents.Count == 0 && !args.Contains("--crash")) && !args.Contains("--openedInExisting"))
        {
            if (preferences!.GetPreference("ShowStartupWindow", true))
            {
                OpenHelloTherePopup();
            }
        }
    }

    private void Owner_OnStartupEvent()
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

    [Command.Internal("PixiEditor.File.OpenRecent", AnalyticsTrack = true)]
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
        MenuItemPath = "FILE/OPEN_FILE", MenuItemOrder = 1, Icon = PixiPerfectIcons.FileText, AnalyticsTrack = true)]
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
        "OPEN_FILE_FROM_CLIPBOARD_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.HasImageInClipboard",
        AnalyticsTrack = true)]
    public void OpenFromClipboard()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var images = await ClipboardController.GetImagesFromClipboard();

            foreach (var dataImage in images)
            {
                if (File.Exists(dataImage.Name))
                {
                    OpenRegularImage(dataImage.Image, null);
                    continue;
                }

                OpenRegularImage(dataImage.Image, null);
            }
        });
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
            else if (IsCustomFormat(path))
            {
                OpenCustomFormat(path, associatePath);
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

    private bool IsCustomFormat(string path)
    {
        string extension = Path.GetExtension(path);
        return documentBuilders.Any(x => x.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
    }

    private void OpenCustomFormat(string path, bool associatePath)
    {
        IDocumentBuilder builder = documentBuilders.First(x =>
            x.Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));

        if (!File.Exists(path))
        {
            NoticeDialog.Show("FILE_NOT_FOUND", "FAILED_TO_OPEN_FILE");
            return;
        }

        try
        {
            DocumentViewModel document = DocumentViewModel.Build(docBuilder => builder.Build(docBuilder, path));
            AddDocumentViewModelToTheSystem(document);

            if (associatePath)
            {
                document.FullFilePath = path;
            }

            AddRecentlyOpened(document.FullFilePath);
        }
        catch (Exception ex)
        {
            NoticeDialog.Show("FAILED_TO_OPEN_FILE", "ERROR");
            Console.WriteLine(ex);
            CrashHelper.SendExceptionInfo(ex);
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
                    image,
                    ColorSpace.CreateSrgbLinear(),
                    out int id)
                .WithOutputNode(id, "Output")
            ));

        if (associatePath)
        {
            doc.FullFilePath = path;
        }

        AddRecentlyOpened(path);

        var fileType = SupportedFilesHelper.ParseImageFormat(Path.GetExtension(path));

        if (fileType != null)
        {
            var fileSize = new FileInfo(path).Length;
            Analytics.SendOpenFile(fileType, fileSize, doc.SizeBindable);
        }
        else
        {
            CrashHelper.SendExceptionInfo(new InvalidFileTypeException(default,
                $"Invalid file type '{fileType}'"));
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
                    surface,
                    ColorSpace.CreateSrgbLinear(),
                    out int id)
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
                        new VecI(newFile.Width, newFile.Height),
                        ColorSpace.CreateSrgbLinear(),
                        out int id)
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
        MenuItemPath = "FILE/SAVE_PIXI", MenuItemOrder = 3, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "SAVE_AS", "SAVE_IMAGE_AS", CanExecute = "PixiEditor.HasDocument",
        Key = Key.S, Modifiers = KeyModifiers.Control | KeyModifiers.Shift, Icon = PixiPerfectIcons.Save,
        MenuItemPath = "FILE/SAVE_AS_PIXI", MenuItemOrder = 4, AnalyticsTrack = true)]
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
            ExportConfig config = new ExportConfig(document.SizeBindable);
            var result = await Exporter.TrySaveWithDialog(document, config, null);
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
            ExportConfig config = new ExportConfig(document.SizeBindable);
            var result = await Exporter.TrySaveAsync(document, document.FullFilePath, config, null);
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
        MenuItemPath = "FILE/EXPORT_IMG", MenuItemOrder = 5, Icon = PixiPerfectIcons.Image, AnalyticsTrack = true)]
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
                ExportJob job = new ExportJob();
                ProgressDialog dialog = new ProgressDialog(job, MainWindow.Current);

                Task.Run(async () =>
                {
                    var result =
                        await Exporter.TrySaveUsingDataFromDialog(doc, info.FilePath, info.ChosenFormat,
                            info.ExportConfig,
                            job);

                    if (result.result == SaveResult.Success)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            IOperatingSystem.Current.OpenFolder(result.finalPath);
                        });
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            ShowSaveError((DialogSaveResult)result.result);
                        });
                    }
                });

                await dialog.ShowDialog();
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
                NoticeDialog.Show("ERROR_SAVE_LOCATION", "ERROR");
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
                NoticeDialog.Show("UNKNOWN_ERROR_SAVING", "ERROR");
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
