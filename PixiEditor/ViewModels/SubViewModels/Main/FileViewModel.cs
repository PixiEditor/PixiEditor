using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Parser;
using PixiEditor.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class FileViewModel : SubViewModel<ViewModelMain>
    {
        private bool hasRecent;

        public RelayCommand OpenNewFilePopupCommand { get; set; }

        public RelayCommand SaveDocumentCommand { get; set; }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand ExportFileCommand { get; set; } // Command that is used to save file

        public RelayCommand OpenRecentCommand { get; set; }

        public RelayCommand RemoveRecentlyOpenedCommand { get; set; }

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

        public FileViewModel(ViewModelMain owner)
            : base(owner)
        {
            OpenNewFilePopupCommand = new RelayCommand(OpenNewFilePopup);
            SaveDocumentCommand = new RelayCommand(SaveDocument, Owner.DocumentIsNotNull);
            OpenFileCommand = new RelayCommand(Open);
            ExportFileCommand = new RelayCommand(ExportFile, CanSave);
            OpenRecentCommand = new RelayCommand(OpenRecent);
            RemoveRecentlyOpenedCommand = new RelayCommand(RemoveRecentlyOpened);
            Owner.OnStartupEvent += Owner_OnStartupEvent;
            RecentlyOpened = new RecentlyOpenedCollection(GetRecentlyOpenedDocuments());

            if (RecentlyOpened.Count > 0)
            {
                HasRecent = true;
            }

            IPreferences.Current.AddCallback("MaxOpenedRecently", UpdateMaxRecentlyOpened);
        }

        public void OpenRecent(object parameter)
        {
            string path = (string)parameter;

            foreach (Document document in Owner.BitmapManager.Documents)
            {
                if (document.DocumentFilePath == path)
                {
                    Owner.BitmapManager.ActiveDocument = document;
                    return;
                }
            }

            if (!File.Exists(path))
            {
                NoticeDialog.Show("The file does not exist", "Failed to open the file");
                RecentlyOpened.Remove(path);
                return;
            }

            Open((string)parameter);
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
        public void OpenNewFilePopup(object parameter)
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
            Owner.BitmapManager.Documents.Add(new Document(width, height));
            Owner.BitmapManager.ActiveDocument = Owner.BitmapManager.Documents[^1];
            if (addBaseLayer)
            {
                Owner.BitmapManager.ActiveDocument.AddNewLayer("Base Layer");
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
                Owner.BitmapManager.ActiveDocument.DocumentFilePath = path;
                Owner.BitmapManager.ActiveDocument.AddNewLayer(
                    "Image",
                    Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight));
                Owner.BitmapManager.ActiveDocument.UpdatePreviewImage();
            }
        }

        public void SaveDocument(bool asNew)
        {
            SaveDocument(parameter: asNew ? "asnew" : null);
        }

        public void OpenAny()
        {
            Open((object)null);
        }

        public void Open(string path)
        {
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
            var file = args.FirstOrDefault(x => Importer.IsSupportedFile(x) && File.Exists(x));
            if (file != null)
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
                
        private void Open(object property)
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

                    if (Owner.BitmapManager.Documents.Count > 0)
                    {
                        Owner.BitmapManager.ActiveDocument = Owner.BitmapManager.Documents.Last();
                    }
                }
            }
        }

        private void OpenDocument(string path)
        {
            Document document = Importer.ImportDocument(path);

            if (Owner.BitmapManager.Documents.Select(x => x.DocumentFilePath).All(y => y != path))
            {
                Owner.BitmapManager.Documents.Add(document);
                Owner.BitmapManager.ActiveDocument = Owner.BitmapManager.Documents.Last();
            }
            else
            {
                Owner.BitmapManager.ActiveDocument = Owner.BitmapManager.Documents.First(y => y.DocumentFilePath == path);
            }
        }

        private void SaveDocument(object parameter)
        {
            bool paramIsAsNew = parameter != null && parameter.ToString()?.ToLower() == "asnew";
            if (paramIsAsNew ||
                string.IsNullOrEmpty(Owner.BitmapManager.ActiveDocument.DocumentFilePath)) 
            {
                Owner.BitmapManager.ActiveDocument.SaveWithDialog();
            }
            else
            {
                Owner.BitmapManager.ActiveDocument.Save();
            }
        }

        /// <summary>
        ///     Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter">CommandProperty.</param>
        private void ExportFile(object parameter)
        {
            ViewModelMain.Current.ActionDisplay = "";
            WriteableBitmap bitmap = Owner.BitmapManager.ActiveDocument.Renderer.FinalBitmap;
            Exporter.Export(bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        }

        /// <summary>
        ///     Returns true if file save is possible.
        /// </summary>
        /// <param name="property">CommandProperty.</param>
        /// <returns>True if active document is not null.</returns>
        private bool CanSave(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null;
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
