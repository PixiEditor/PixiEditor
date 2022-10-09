using System.IO;
using System.Windows.Input;
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

    /// <summary>
    ///     Generates new Layer and sets it as active one.
    /// </summary>
    /// <param name="parameter">CommandParameter.</param>
    [Command.Basic("PixiEditor.File.New", "New image", "Create new image", Key = Key.N, Modifiers = ModifierKeys.Control)]
    public void OpenNewFilePopup()
    {
        NewFileDialog newFile = new NewFileDialog();
        if (newFile.ShowDialog())
        {
            NewDocument(new VecI(newFile.Width, newFile.Height));
        }
    }

    public void OpenHelloTherePopup()
    {
        new HelloTherePopup(this).Show();
    }

    public DocumentViewModel NewDocument(VecI size, bool addBaseLayer = true)
    {
        DocumentViewModel doc = new DocumentViewModel();
        Owner.DocumentManagerSubViewModel.Documents.Add(doc);

        if (doc.SizeBindable != size)
            doc.Operations.ResizeCanvas(size, ResizeAnchor.TopLeft);
        if (addBaseLayer)
            doc.Operations.CreateStructureMember(StructureMemberType.Layer);
        doc.Operations.ClearUndo();
        doc.MarkAsSaved();
        Owner.WindowSubViewModel.CreateNewViewport(doc);
        Owner.WindowSubViewModel.MakeDocumentViewportActive(doc);

        return doc;
    }

    /// <summary>
    ///     Opens file from path.
    /// </summary>
    /// <param name="path">Path to file.</param>
    public void OpenFile(string path)
    {
        ImportFileDialog dialog = new ImportFileDialog();

        if (path != null && File.Exists(path))
        {
            dialog.FilePath = path;
        }

        if (dialog.ShowDialog())
        {
            DocumentViewModel doc = NewDocument(new(dialog.FileWidth, dialog.FileHeight), false);
            doc.FullFilePath = path;

            Guid? guid = doc.Operations.CreateStructureMember(StructureMemberType.Layer, "Image");
            Surface surface = Importer.ImportImage(dialog.FilePath, new VecI(dialog.FileWidth, dialog.FileHeight));
            RectD corners = RectD.FromTwoPoints(new VecD(0, 0), new VecD(dialog.FileWidth, dialog.FileHeight));
            doc.Operations.DrawImage(surface, new ShapeCorners(corners), guid.Value, true, false);
        }
    }

    [Command.Basic("PixiEditor.File.Open", "Open", "Open file", Key = Key.O, Modifiers = ModifierKeys.Control)]
    public void Open(string path)
    {
        if (path == null)
        {
            Open();
            return;
        }

        try
        {
            if (path.EndsWith(".pixi"))
            {
                OpenDocument(path);
            }
            else
            {
                OpenFile(path);
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

    private void Owner_OnStartupEvent(object sender, System.EventArgs e)
    {
        List<string> args = StartupArgs.Args;
        string file = args.FirstOrDefault(x => Importer.IsSupportedFile(x) && File.Exists(x));
        if (file != null)
        {
            Open(file);
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

        foreach (DocumentViewModel document in Owner.DocumentManagerSubViewModel.Documents)
        {
            if (document.FullFilePath is not null && document.FullFilePath == path)
            {
                Owner.WindowSubViewModel.MakeDocumentViewportActive(document);
                return;
            }
        }

        if (!File.Exists(path))
        {
            NoticeDialog.Show("The file does not exist", "Failed to open the file");
            RecentlyOpened.Remove(path);
            IPreferences.Current.UpdateLocalPreference("RecentlyOpened", RecentlyOpened.Select(x => x.FilePath));
            return;
        }

        Open((string)parameter);
    }

    public void Open()
    {
        string filter = SupportedFilesHelper.BuildOpenFilter();

        OpenFileDialog dialog = new OpenFileDialog
        {
            Filter = filter,
            FilterIndex = 0
        };

        if ((bool)dialog.ShowDialog())
        {
            if (Importer.IsSupportedFile(dialog.FileName))
            {
                Open(dialog.FileName);

                if (Owner.DocumentManagerSubViewModel.Documents.Count > 0)
                {
                    Owner.WindowSubViewModel.MakeDocumentViewportActive(Owner.DocumentManagerSubViewModel.Documents[^1]);
                }
            }
        }
    }

    private void OpenDocument(string path)
    {
        DocumentViewModel document = Importer.ImportDocument(path);
        DocumentManagerViewModel manager = Owner.DocumentManagerSubViewModel;
        if (manager.Documents.Select(x => x.FullFilePath).All(y => y != path))
        {
            manager.Documents.Add(document);
            Owner.WindowSubViewModel.CreateNewViewport(document);
            Owner.WindowSubViewModel.MakeDocumentViewportActive(document);
        }
        else
        {
            Owner.WindowSubViewModel.MakeDocumentViewportActive(manager.Documents.First(y => y.FullFilePath == path));
        }
    }

    [Command.Basic("PixiEditor.File.Save", false, "Save", "Save image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control)]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "Save as...", "Save image as new", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    public bool SaveActiveDocument(bool asNew)
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return false;
        return SaveDocument(doc, asNew);
    }

    public bool SaveDocument(DocumentViewModel document, bool asNew)
    {
        if (asNew || string.IsNullOrEmpty(document.FullFilePath))
        {
            //doc.SaveWithDialog();
        }
        else
        {
            //doc.Save();
        }
        return false;
    }

    /// <summary>
    ///     Generates export dialog or saves directly if save data is known.
    /// </summary>
    /// <param name="parameter">CommandProperty.</param>
    [Command.Basic("PixiEditor.File.Export", "Export", "Export image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)]
    public void ExportFile()
    {
        /*
        ViewModelMain.Current.ActionDisplay = "";
        WriteableBitmap bitmap = Owner.BitmapManager.ActiveDocument.Renderer.FinalBitmap;
        Exporter.Export(bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        */
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
