using System;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Input;
using System.Windows.Shapes;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Exceptions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.Localization;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.Dialogs;
using Path = System.IO.Path;

namespace PixiEditor.ViewModels.SubViewModels.Main;

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
            RaisePropertyChanged(nameof(HasRecent));
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

        IPreferences.Current.AddCallback(PreferencesConstants.MaxOpenedRecently, UpdateMaxRecentlyOpened);
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

        int maxCount = IPreferences.Current.GetPreference(PreferencesConstants.MaxOpenedRecently, PreferencesConstants.MaxOpenedRecentlyDefault);

        while (RecentlyOpened.Count > maxCount)
        {
            RecentlyOpened.RemoveAt(RecentlyOpened.Count - 1);
        }

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.RecentlyOpened, RecentlyOpened.Select(x => x.FilePath));
    }

    [Command.Internal("PixiEditor.File.RemoveRecent")]
    public void RemoveRecentlyOpened(string path)
    {
        if (!RecentlyOpened.Contains(path))
        {
            return;
        }

        RecentlyOpened.Remove(path);
        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.RecentlyOpened, RecentlyOpened.Select(x => x.FilePath));
    }

    private void OpenHelloTherePopup()
    {
        new HelloTherePopup(this).Show();
    }

    private void Owner_OnStartupEvent(object sender, System.EventArgs e)
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
                    ReopenTempFiles();
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
            CrashHelper.SendExceptionInfoToWebhook(exc);
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

    public void OpenFromReport(CrashReport report, out bool showMissingFilesDialog)
    {
        var documents = report.RecoverDocuments();
        
        var i = 0;

        Exception firstException = null;
        Exception secondException = null;
        Exception thirdException = null;
        
        foreach (var document in documents)
        {
            try
            {
                OpenRecoveredDotPixi(document.Path.OriginalPath, document.Path.AutosavePath, document.Path.GetAutosaveGuid(), document.GetRecoveredBytes());
                i++;
            }
            catch (Exception e)
            {
                firstException = e;
                
                try
                {
                    OpenFromPath(document.Path.AutosavePath, false);
                    
                }
                catch (Exception deepE)
                {
                    secondException = deepE;
                    
                    try
                    {
                        OpenRecoveredDotPixi(document.Path.OriginalPath, document.Path.AutosavePath, document.Path.GetAutosaveGuid(), document.GetAutosaveBytes());
                    }
                    catch (Exception veryDeepE)
                    {
                        thirdException = veryDeepE;
                    }
                }
            }

            var exceptions = new[] { firstException, secondException, thirdException };
            CrashHelper.SendExceptionInfoToWebhook(new AggregateException(exceptions.Where(x => x != null).ToArray()));
        }

        showMissingFilesDialog = documents.Count != i;
    }

    private void ReopenTempFiles()
    {
        var preferences = Owner.Preferences;
        var files = preferences.GetLocalPreference<AutosaveFilePathInfo[]>(PreferencesConstants.UnsavedNextSessionFiles);

        if (files == null)
            return;
        
        foreach (var file in files)
        {
            try
            {
                if (file.AutosavePath != null)
                {
                    var document = OpenFromPath(file.AutosavePath, false);
                    document.FullFilePath = file.OriginalPath;

                    if (file.AutosavePath != null)
                    {
                        document.AutosaveViewModel.SetTempFileGuidAndLastSavedPath(file.GetAutosaveGuid()!.Value, file.AutosavePath);
                    }
                }
                else
                {
                    OpenFromPath(file.OriginalPath);
                }
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfoToWebhook(e);
            }
        }
        
        preferences.UpdateLocalPreference(PreferencesConstants.UnsavedNextSessionFiles, Array.Empty<string>());
    }

    [Command.Internal("PixiEditor.File.OpenRecent")]
    public void OpenRecent(object parameter)
    {
        string path = (string)parameter;
        if (!File.Exists(path))
        {
            NoticeDialog.Show("FILE_NOT_FOUND", "FAILED_TO_OPEN_FILE");
            RecentlyOpened.Remove(path);
            IPreferences.Current.UpdateLocalPreference(PreferencesConstants.RecentlyOpened, RecentlyOpened.Select(x => x.FilePath));
            return;
        }

        OpenFromPath(path);
    }

    [Command.Basic("PixiEditor.File.Open", "OPEN", "OPEN_FILE", Key = Key.O, Modifiers = ModifierKeys.Control)]
    public void OpenFromOpenFileDialog()
    {
        string filter = SupportedFilesHelper.BuildOpenFilter();

        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = filter,
            FilterIndex = 0
        };

        if (!(bool)dialog.ShowDialog() || !Importer.IsSupportedFile(dialog.FileName))
            return;

        OpenFromPath(dialog.FileName);
    }

    [Command.Basic("PixiEditor.File.OpenFileFromClipboard", "OPEN_FILE_FROM_CLIPBOARD", "OPEN_FILE_FROM_CLIPBOARD_DESCRIPTIVE", CanExecute = "PixiEditor.Clipboard.HasImageInClipboard")]
    public void OpenFromClipboard()
    {
        var images = ClipboardController.GetImagesFromClipboard();

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
            if (document.FullFilePath is not null && System.IO.Path.GetFullPath(document.FullFilePath) == System.IO.Path.GetFullPath(path))
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
            return path.EndsWith(".pixi")
                ? OpenDotPixi(path, associatePath)
                : OpenRegularImage(path, associatePath);
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

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    private DocumentViewModel OpenDotPixi(string path, bool associatePath = true)
    {
        var document = Importer.ImportDocument(path, associatePath);
        AddDocumentViewModelToTheSystem(document);
        AddRecentlyOpened(document.FullFilePath);

        return document;
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    public void OpenRecoveredDotPixi(string? originalPath, string? autosavePath, Guid? autosaveGuid, byte[] dotPixiBytes)
    {
        DocumentViewModel document = Importer.ImportDocument(dotPixiBytes, originalPath);
        document.MarkAsUnsaved();

        if (autosavePath != null)
        {
            document.AutosaveViewModel.SetTempFileGuidAndLastSavedPath(autosaveGuid!.Value, autosavePath);
        }
        
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
            .WithLayer(l => l
                .WithName("Image")
                .WithSize(image.Size)
                .WithSurface(image)));

        if (associatePath)
        {
            doc.FullFilePath = path;
        }

        AddRecentlyOpened(path);
        return doc;
    }

    /// <summary>
    /// Opens a regular image file from path, creates a document from it, and adds it to the system.
    /// </summary>
    private void OpenRegularImage(Surface surface, string path)
    {
        DocumentViewModel doc = NewDocument(b => b
            .WithSize(surface.Size)
            .WithLayer(l => l
                .WithName("Image")
                .WithSize(surface.Size)
                .WithSurface(surface)));

        if (path == null)
        {
            return;
        }

        doc.FullFilePath = path;
        AddRecentlyOpened(path);
    }

    [Command.Basic("PixiEditor.File.New", "NEW_IMAGE", "CREATE_NEW_IMAGE", Key = Key.N, Modifiers = ModifierKeys.Control)]
    public void CreateFromNewFileDialog()
    {
        NewFileDialog newFile = new NewFileDialog();
        if (newFile.ShowDialog())
        {
            NewDocument(b => b
                .WithSize(newFile.Width, newFile.Height)
                .WithLayer(l => l
                    .WithName(new LocalizedString("BASE_LAYER_NAME"))
                    .WithSurface(new Surface(new VecI(newFile.Width, newFile.Height)))));
        }
    }

    private DocumentViewModel NewDocument(Action<DocumentViewModelBuilder> builder)
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

    [Command.Basic("PixiEditor.File.Save", false, "SAVE", "SAVE_IMAGE", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control, IconPath = "Save.png")]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "SAVE_AS", "SAVE_IMAGE_AS", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Save.png")]
    public bool SaveActiveDocument(bool asNew)
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return SaveDocument(doc, asNew);
    }

    public bool SaveDocument(DocumentViewModel document, bool asNew)
    {
        string finalPath = null;
        if (asNew || string.IsNullOrEmpty(document.FullFilePath))
        {
            var result = Exporter.TrySaveWithDialog(document, out string path);
            if (result == DialogSaveResult.Cancelled)
                return false;
            if (result != DialogSaveResult.Success)
            {
                ShowSaveError(result);
                return false;
            }
            finalPath = path;
            AddRecentlyOpened(path);
        }
        else
        {
            var result = Exporter.TrySave(document, document.FullFilePath);
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
    [Command.Basic("PixiEditor.File.Export", "EXPORT", "EXPORT_IMAGE", CanExecute = "PixiEditor.HasDocument", Key = Key.E, Modifiers = ModifierKeys.Control)]
    public void ExportFile()
    {
        try
        {
            DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
            if (doc is null)
                return;

            ExportFileDialog info = new ExportFileDialog(doc.SizeBindable);
            if (info.ShowDialog())
            {
                SaveResult result = Exporter.TrySaveUsingDataFromDialog(doc, info.FilePath, info.ChosenFormat, out string finalPath, new(info.FileWidth, info.FileHeight));
                if (result == SaveResult.Success)
                    ProcessHelper.OpenInExplorer(finalPath);
                else
                    ShowSaveError((DialogSaveResult)result);
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

    private void UpdateMaxRecentlyOpened(object parameter)
    {
        int newAmount = (int)parameter;

        if (newAmount >= RecentlyOpened.Count)
        {
            return;
        }

        List<RecentlyOpenedDocument> recentlyOpenedDocuments = new List<RecentlyOpenedDocument>(RecentlyOpened.Take(newAmount));

        RecentlyOpened.Clear();

        foreach (RecentlyOpenedDocument recent in recentlyOpenedDocuments)
        {
            RecentlyOpened.Add(recent);
        }
    }

    private List<RecentlyOpenedDocument> GetRecentlyOpenedDocuments()
    {
        IEnumerable<string> paths = IPreferences.Current.GetLocalPreference(nameof(RecentlyOpened), new JArray()).ToObject<string[]>()
            .Take(IPreferences.Current.GetPreference(PreferencesConstants.MaxOpenedRecently, 8));

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
