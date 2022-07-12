﻿using System.IO;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PixiEditor.ChangeableDocument.Enums;
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
        Owner.DocumentManagerSubViewModel.ActiveDocument = Owner.DocumentManagerSubViewModel.Documents[^1];

        if (doc.SizeBindable != size)
            doc.ResizeCanvas(size, ResizeAnchor.TopLeft);
        if (addBaseLayer)
            doc.CreateStructureMember(StructureMemberType.Layer);
        doc.ClearUndo();
        doc.MarkAsSaved();

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
            /*doc.AddNewLayer(
                "Image",
                Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight));*/
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
                Owner.DocumentManagerSubViewModel.ActiveDocument = document;
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
                    Owner.DocumentManagerSubViewModel.ActiveDocument = Owner.DocumentManagerSubViewModel.Documents[^1];
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
            manager.ActiveDocument = manager.Documents[^1];
        }
        else
        {
            manager.ActiveDocument = manager.Documents.First(y => y.FullFilePath == path);
        }
    }

    [Command.Basic("PixiEditor.File.Save", false, "Save", "Save image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control)]
    [Command.Basic("PixiEditor.File.SaveAsNew", true, "Save as...", "Save image as new", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    public void SaveDocument(bool asNew)
    {
        DocumentViewModel doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;
        if (asNew || string.IsNullOrEmpty(doc.FullFilePath))
        {
            //doc.SaveWithDialog();
        }
        else
        {
            //doc.Save();
        }
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