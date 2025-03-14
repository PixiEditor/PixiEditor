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
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models.DocumentModels.Autosave;
using PixiEditor.Models.ExceptionHandling;
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
        new HelloTherePopup(this).Show();
    }

    private void Owner_OnStartupEvent()
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
                    TryReopenTempAutosavedFiles();
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
        else if ((Owner.DocumentManagerSubViewModel.Documents.Count == 0 && !args.Contains("--crash")) &&
                 !args.Contains("--openedInExisting"))
        {
            if (preferences!.GetPreference("ShowStartupWindow", true))
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
    public DocumentViewModel OpenFromPath(string path, bool associatePath = true)
    {
        if (MakeExistingDocumentActiveIfOpened(path))
            return null;

        try
        {
            if (path.EndsWith(".pixi"))
            {
                return OpenDotPixi(path, associatePath);
            }

            if (IsCustomFormat(path))
            {
                return OpenCustomFormat(path, associatePath);
            }

            return OpenRegularImage(path, associatePath);
        }
        catch (RecoverableException ex)
        {
            NoticeDialog.Show(ex.DisplayMessage, "ERROR");
        }
        catch (OldFileFormatException)
        {
            NoticeDialog.Show("OLD_FILE_FORMAT_DESCRIPTION", "OLD_FILE_FORMAT");
        }

        return null;
    }

    private bool IsCustomFormat(string path)
    {
        string extension = Path.GetExtension(path);
        return documentBuilders.Any(x => x.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
    }

    private DocumentViewModel? OpenCustomFormat(string path, bool associatePath)
    {
        IDocumentBuilder builder = documentBuilders.First(x =>
            x.Extensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase));

        if (!File.Exists(path))
        {
            NoticeDialog.Show("FILE_NOT_FOUND", "FAILED_TO_OPEN_FILE");
            return null;
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
            return document;
        }
        catch (Exception ex)
        {
            NoticeDialog.Show("FAILED_TO_OPEN_FILE", "ERROR");
            Console.WriteLine(ex);
            CrashHelper.SendExceptionInfo(ex);
        }

        return null;
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    private DocumentViewModel OpenDotPixi(string path, bool associatePath = true)
    {
        DocumentViewModel document = Importer.ImportDocument(path, associatePath);

        AddDocumentViewModelToTheSystem(document);
        AddRecentlyOpened(document.FullFilePath);

        var fileSize = new FileInfo(path).Length;
        Analytics.SendOpenFile(PixiFileType.PixiFile, fileSize, document.SizeBindable);

        return document;
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    public void OpenRecoveredDotPixi(string? originalPath, string? autosavePath, Guid? autosaveGuid,
        byte[] dotPixiBytes)
    {
        DocumentViewModel document = Importer.ImportDocument(dotPixiBytes, originalPath);
        document.MarkAsUnsaved();

        if (autosavePath != null)
        {
            document.AutosaveViewModel.SetTempFileGuidAndLastSavedPath(autosaveGuid!.Value, autosavePath);
        }

        AddDocumentViewModelToTheSystem(document);
    }

    public void OpenFromPixiBytes(byte[] bytes)
    {
        DocumentViewModel document = Importer.ImportDocument(bytes, null);
        AddDocumentViewModelToTheSystem(document);
    }

    /// <summary>
    /// Opens a regular image file from path, creates a document from it, and adds it to the system.
    /// </summary>
    private DocumentViewModel OpenRegularImage(string path, bool associatePath)
    {
        var image = Importer.ImportImage(path, VecI.NegativeOne);

        if (image == null) return null;

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

        return doc;
    }

    public void OpenFromReport(CrashReport report, out bool showMissingFilesDialog)
    {
        var documents = report.RecoverDocuments(out var info);

        var i = 0;

        Exception firstException = null;
        Exception secondException = null;
        Exception thirdException = null;

        foreach (var document in documents)
        {
            try
            {
                OpenRecoveredDotPixi(document.OriginalPath, document.AutosavePath,
                    AutosaveHelper.GetAutosaveGuid(document.AutosavePath), document.GetRecoveredBytes());
                i++;
            }
            catch (Exception e)
            {
                firstException = e;

                try
                {
                    OpenFromPath(document.AutosavePath, false);
                }
                catch (Exception deepE)
                {
                    secondException = deepE;

                    try
                    {
                        OpenRecoveredDotPixi(document.OriginalPath, document.AutosavePath,
                            AutosaveHelper.GetAutosaveGuid(document.AutosavePath), document.TryGetAutoSaveBytes());
                    }
                    catch (Exception veryDeepE)
                    {
                        thirdException = veryDeepE;
                    }
                }
            }

            var exceptions = new[] { firstException, secondException, thirdException };
            CrashHelper.SendExceptionInfo(new AggregateException(exceptions.Where(x => x != null).ToArray()));
        }

        showMissingFilesDialog = documents.Count != i;
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

    private void TryReopenTempAutosavedFiles()
    {
        var preferences = Owner.Preferences;

        // Todo sure, no session saving, but shouldn't we still load backups in case of unexpected shutdown?
        // it probably should be handled elsewhere
        if (!preferences.GetPreference<bool>(PreferencesConstants.SaveSessionStateEnabled,
                PreferencesConstants.SaveSessionStateDefault))
            return;

        var history =
            preferences.GetLocalPreference<List<AutosaveHistorySession>>(PreferencesConstants.AutosaveHistory);

        // There are no autosave attempts .. but what if the user has just launched pixieditor for the first time,
        // and it unexpectedly closed before auto saving anything. They could've still had some files open, and they won't be reopened in this session
        // I'll say this is by design
        if (history is null || history.Count == 0)
            return;
        var lastSession = history[^1];
        if (lastSession.AutosaveEntries.Count == 0)
            return;

        bool shutdownWasUnexpected = lastSession.AutosaveEntries.All(a => a.Type != AutosaveHistoryType.OnClose);
        if (shutdownWasUnexpected)
        {
            LoadFromUnexpectedShutdown(lastSession);
            return;
        }

        var nextSessionFiles = preferences.GetLocalPreference<SessionFile[]>(PreferencesConstants.NextSessionFiles, []);
        List<List<AutosaveHistoryEntry>> perDocumentHistories = (
            from entry in lastSession.AutosaveEntries
            where nextSessionFiles.Any(a =>
                a.AutosaveFilePath == AutosaveHelper.GetAutosavePath(entry.TempFileGuid) || entry.Type != AutosaveHistoryType.OnClose)
            group entry by entry.TempFileGuid
            into entryGroup
            select entryGroup.OrderBy(a => a.DateTime).ToList()
        ).ToList();

        var toLoad = nextSessionFiles.ToList();

        foreach (var documentHistory in perDocumentHistories)
        {
            AutosaveHistoryEntry lastEntry = documentHistory[^1];
            try
            {
                if (lastEntry.Type != AutosaveHistoryType.OnClose)
                {
                    // unexpected shutdown happened, this file wasn't saved on close, but we supposedly have a backup
                    LoadNewest(lastEntry);
                    toLoad.RemoveAll(a =>
                        a.AutosaveFilePath == AutosaveHelper.GetAutosavePath(lastEntry.TempFileGuid)
                        || a.OriginalFilePath == lastEntry.OriginalPath);
                }
                else if (lastEntry.Result == AutosaveHistoryResult.SavedBackup)
                {
                    LoadFromAutosave(lastEntry);
                    toLoad.RemoveAll(a =>
                        a.AutosaveFilePath == AutosaveHelper.GetAutosavePath(lastEntry.TempFileGuid)
                        || a.OriginalFilePath == lastEntry.OriginalPath);
                }
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfo(e);
            }
        }

        foreach (var file in toLoad)
        {
            if (file.OriginalFilePath != null)
            {
                OpenFromPath(file.OriginalFilePath);
            }
        }

        Owner.AutosaveViewModel.CleanupAutosavedFilesAndHistory();
        preferences.UpdateLocalPreference(PreferencesConstants.NextSessionFiles, Array.Empty<SessionFile>());
    }

    private void LoadFromUnexpectedShutdown(AutosaveHistorySession lastSession)
    {
        List<List<AutosaveHistoryEntry>> lastBackups = (
            from entry in lastSession.AutosaveEntries
            group entry by entry.TempFileGuid
            into entryGroup
            select entryGroup.OrderBy(a => a.DateTime).ToList()
        ).ToList();

        try
        {
            foreach (var backup in lastBackups)
            {
                AutosaveHistoryEntry lastEntry = backup[^1];
                LoadNewest(lastEntry);
            }

            OptionsDialog<LocalizedString> dialog = new OptionsDialog<LocalizedString>("UNEXPECTED_SHUTDOWN",
                new LocalizedString("UNEXPECTED_SHUTDOWN_MSG"),
                MainWindow.Current!)
            {
                { "OPEN_AUTOSAVES", _ => { IOperatingSystem.Current.OpenFolder(Paths.PathToUnsavedFilesFolder); } },
                "OK"
            };
            dialog.ShowDialog(true);
        }
        catch (Exception e)
        {
            CrashHelper.SendExceptionInfo(e);
        }
    }

    private void LoadNewest(AutosaveHistoryEntry lastEntry)
    {
        bool loadFromUserFile = false;

        if (lastEntry.OriginalPath != null && File.Exists(lastEntry.OriginalPath))
        {
            DateTime saveFileWriteTime = File.GetLastWriteTime(lastEntry.OriginalPath);
            DateTime autosaveWriteTime = lastEntry.DateTime;

            loadFromUserFile = saveFileWriteTime > autosaveWriteTime;
        }

        if (loadFromUserFile)
        {
            OpenFromPath(lastEntry.OriginalPath);
        }
        else
        {
            LoadFromAutosave(lastEntry);
        }
    }

    private void LoadFromAutosave(AutosaveHistoryEntry entry)
    {
        string path = AutosaveHelper.GetAutosavePath(entry.TempFileGuid);
        if (path == null || !File.Exists(path))
        {
            // TODO: Notice user when non-blocking notification system is implemented
            return;
        }

        var document = OpenFromPath(path, false);
        document.AutosaveViewModel.SetTempFileGuidAndLastSavedPath(entry.TempFileGuid, path);
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
