using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Parser;
using PixiEditor.Views.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Services;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.File", "File")]
    public class FileViewModel : SubViewModel<ViewModelMain>
    {
        private readonly DocumentProvider _doc;
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

        public RecentlyOpenedCollection RecentlyOpened { get; set; } = new RecentlyOpenedCollection();

        public FileViewModel(ViewModelMain owner, DocumentProvider provider)
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
                NewDocument(newFile.Width, newFile.Height);
            }
        }

        public void OpenHelloTherePopup()
        {
            new HelloTherePopup(this).Show();
        }

        public void NewDocument(int width, int height, bool addBaseLayer = true)
        {
            Document document = new Document(width, height);
            _doc.GetDocuments().Add(document);
            Owner.BitmapManager.ActiveDocument = document;
            if (addBaseLayer)
            {
                document.AddNewLayer("Base Layer");
            }

            Owner.ResetProgramStateValues();
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
                NewDocument(dialog.FileWidth, dialog.FileHeight, false);
                _doc.GetDocument().DocumentFilePath = path;
                _doc.GetDocument().AddNewLayer(
                    "Image",
                    Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight));
                _doc.GetDocument().UpdatePreviewImage();
            }
        }

        [Command.Basic("PixiEditor.File.Open", "Open", "Open image", Key = Key.O, Modifiers = ModifierKeys.Control)]
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

                Owner.ResetProgramStateValues();
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
            var args = Environment.GetCommandLineArgs();
            var file = args.Last();
            if (Importer.IsSupportedFile(file) && File.Exists(file))
            {
                Open(file);
            }
            else if (Owner.BitmapManager.Documents.Count == 0 || !args.Contains("--crash"))
            {
                if (IPreferences.Current.GetPreference("ShowStartupWindow", true))
                {
                    OpenHelloTherePopup();
                }
            }
        }

        public void Open()
        {
            var filter = SupportedFilesHelper.BuildOpenFilter();

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

                    if (_doc.GetDocuments().Count > 0)
                    {
                        Owner.BitmapManager.ActiveDocument = _doc.GetDocuments().Last();
                    }
                }
            }
        }

        private void OpenDocument(string path)
        {
            Document document = _doc.GetDocuments().FirstOrDefault(x => x.DocumentFilePath == path);

            if (document is null)
            {
                document = Importer.ImportDocument(path);
                _doc.GetDocuments().Add(document);
            }
            
            Owner.BitmapManager.ActiveDocument = document;
        }

        [Command.Basic("PixiEditor.File.Save", false, "Save", "Save image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control)]
        [Command.Basic("PixiEditor.File.SaveAsNew", true, "Save as...", "Save image as new", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
        public void SaveDocument(bool asNew)
        {
            if (asNew || string.IsNullOrEmpty(_doc.GetDocument().DocumentFilePath)) 
            {
                _doc.GetDocument().SaveWithDialog();
            }
            else
            {
                _doc.GetDocument().Save();
            }
        }

        /// <summary>
        ///     Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter">CommandProperty.</param>
        [Command.Basic("PixiEditor.File.Export", "Export", "Export image", CanExecute = "PixiEditor.HasDocument", Key = Key.S, Modifiers = ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)]
        public void ExportFile()
        {
            ViewModelMain.Current.ActionDisplay = "";
            WriteableBitmap bitmap = _doc.GetRenderer().FinalBitmap;
            Exporter.Export(bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        }

        private void UpdateMaxRecentlyOpened(object parameter)
        {
            int newAmount = (int)parameter;

            if (newAmount >= RecentlyOpened.Count)
            {
                return;
            }

            var recentlyOpeneds = new List<RecentlyOpenedDocument>(RecentlyOpened.Take(newAmount));

            RecentlyOpened.Clear();

            foreach (var recent in recentlyOpeneds)
            {
                RecentlyOpened.Add(recent);
            }
        }

        private List<RecentlyOpenedDocument> GetRecentlyOpenedDocuments()
        {
            var paths = IPreferences.Current.GetLocalPreference(nameof(RecentlyOpened), new JArray()).ToObject<string[]>()
                .Take(IPreferences.Current.GetPreference("MaxOpenedRecently", 8));

            List<RecentlyOpenedDocument> documents = new List<RecentlyOpenedDocument>();

            foreach (string path in paths)
            {
                documents.Add(new RecentlyOpenedDocument(path));
            }

            return documents;
        }
    }
}
