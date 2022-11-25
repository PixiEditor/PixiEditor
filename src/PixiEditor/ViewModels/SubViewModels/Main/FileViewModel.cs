using System.IO;
using System.Windows.Input;
using System.Windows.Shapes;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.File", "File")]
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

        IPreferences.Current.AddCallback("MaxOpenedRecently", UpdateMaxRecentlyOpened);
    }

    public void RemoveRecentlyOpened(object parameter)
    {
        if (RecentlyOpened.Contains((string)parameter))
        {
            RecentlyOpened.Remove((string)parameter);
        }
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
        else if ((Owner.DocumentManagerSubViewModel.Documents.Count == 0
                  || !args.Contains("--crash")) && !args.Contains("--openedInExisting"))
        {
            if (IPreferences.Current.GetPreference("ShowStartupWindow", true))
            {
                OpenHelloTherePopup();
            }
        }
    }

    [Command.Internal("PixiEditor.File.OpenRecent")]
    public void OpenRecent(object parameter)
    {
        string path = (string)parameter;
        if (!File.Exists(path))
        {
            NoticeDialog.Show("The file does not exist", "Failed to open the file");
            RecentlyOpened.Remove(path);
            IPreferences.Current.UpdateLocalPreference("RecentlyOpened", RecentlyOpened.Select(x => x.FilePath));
            return;
        }

        OpenFromPath(path);
    }

    [Command.Basic("PixiEditor.File.Open", "Open", "Open file", Key = Key.O, Modifiers = ModifierKeys.Control)]
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
    /// <param name="path"></param>
    public void OpenFromPath(string path)
    {
        if (MakeExistingDocumentActiveIfOpened(path))
            return;

        try
        {
            if (path.EndsWith(".pixi"))
            {
                OpenDotPixi(path);
            }
            else
            {
                OpenRegularImage(path);
            }
        }
        catch (CorruptedFileException ex)
        {
            NoticeDialog.Show(ex.Message, "Failed to open the file");
        }
        catch (OldFileFormatException)
        {
            NoticeDialog.Show("This .pixi file uses the old format,\n which is no longer supported and can't be opened.", "Old file format");
        }
    }

    /// <summary>
    /// Opens a .pixi file from path, creates a document from it, and adds it to the system
    /// </summary>
    private void OpenDotPixi(string path)
    {
        DocumentViewModel document = Importer.ImportDocument(path);
        AddDocumentViewModelToTheSystem(document);
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
    private void OpenRegularImage(string path)
    {
        ImportFileDialog dialog = new ImportFileDialog();

        if (path != null && File.Exists(path))
        {
            dialog.FilePath = path;
        }

        if (dialog.ShowDialog())
        {
            DocumentViewModel doc = NewDocument(b => b
                .WithSize(dialog.FileWidth, dialog.FileHeight)
                .WithLayer(l => l
                    .WithName("Image")
                    .WithSize(dialog.FileWidth, dialog.FileHeight)
                    .WithSurface(Importer.ImportImage(dialog.FilePath, new VecI(dialog.FileWidth, dialog.FileHeight)))));
            doc.FullFilePath = path;
        }
    }

    [Command.Basic("PixiEditor.File.New", "New image", "Create new image", Key = Key.N, Modifiers = ModifierKeys.Control)]
    public void CreateFromNewFileDialog()
    {
        NewFileDialog newFile = new NewFileDialog();
        if (newFile.ShowDialog())
        {
            NewDocument(b => b
                .WithSize(newFile.Width, newFile.Height)
                .WithLayer(l => l
                    .WithName("Base Layer")
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

    [Command.Basic("PixiEditor.File.Save", false, "Save", "Save image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control, IconPath = "Save.png")]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "Save as...", "Save image as new", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Save.png")]
    public bool SaveActiveDocument(bool asNew)
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return SaveDocument(doc, asNew);
    }

    public bool SaveDocument(DocumentViewModel document, bool asNew)
    {
        string path = "";
        bool success = false;
        if (asNew || string.IsNullOrEmpty(document.FullFilePath))
        {
            success = Exporter.SaveAsEditableFileWithDialog(document, out path);
        }
        else
        {
            path = Exporter.SaveAsEditableFile(document, document.FullFilePath);
            success = path != null;
        }

        if (success)
        {
            document.FullFilePath = path;
            document.MarkAsSaved();
        }

        return success;
    }

    /// <summary>
    ///     Generates export dialog or saves directly if save data is known.
    /// </summary>
    /// <param name="parameter">CommandProperty.</param>
    [Command.Basic("PixiEditor.File.Export", "Export", "Export image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)]
    public void ExportFile()
    {
        ViewModelMain.Current.ActionDisplay = "";
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        var bitmap = doc.Bitmaps[ChunkResolution.Full];
        if (Exporter.Export(bitmap, new VecI(bitmap.PixelWidth, bitmap.PixelHeight), out string path))
        {
            ProcessHelper.OpenInExplorer(path);
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
            .Take(IPreferences.Current.GetPreference("MaxOpenedRecently", 8));

        List<RecentlyOpenedDocument> documents = new List<RecentlyOpenedDocument>();

        foreach (string path in paths)
        {
            documents.Add(new RecentlyOpenedDocument(path));
        }

        return documents;
    }
}
