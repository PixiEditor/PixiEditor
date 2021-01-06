using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.Parser;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class FileViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand OpenNewFilePopupCommand { get; set; }

        public RelayCommand SaveDocumentCommand { get; set; }

        public RelayCommand OpenFileCommand { get; set; }

        public RelayCommand ExportFileCommand { get; set; } // Command that is used to save file

        public FileViewModel(ViewModelMain owner)
            : base(owner)
        {
            OpenNewFilePopupCommand = new RelayCommand(OpenNewFilePopup);
            SaveDocumentCommand = new RelayCommand(SaveDocument, Owner.DocumentIsNotNull);
            OpenFileCommand = new RelayCommand(Open);
            ExportFileCommand = new RelayCommand(ExportFile, CanSave);
            Owner.OnStartupEvent += Owner_OnStartupEvent;
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
            }
        }

        public void SaveDocument(bool asNew)
        {
            SaveDocument(parameter: asNew ? "asnew" : null);
        }

        private void Owner_OnStartupEvent(object sender, System.EventArgs e)
        {
            var lastArg = Environment.GetCommandLineArgs().Last();
            if (Importer.IsSupportedFile(lastArg) && File.Exists(lastArg))
            {
                Open(lastArg);
            }
            else
            {
                if (PreferencesSettings.GetPreference("ShowNewFilePopupOnStartup", true))
                {
                    OpenNewFilePopup(null);
                }
            }
        }

        private void Open(string path)
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
                MessageBox.Show(ex.Message, "Failed to open file.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OldFileFormatException)
            {
                MessageBoxResult result = MessageBox.Show("This pixi file uses the old file format and is insecure.\nOnly continue if you trust the source of the file", "Old file format", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        OpenDocument(path, true);
                    }
                    catch (CorruptedFileException ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to open file.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Open(object property)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|PixiEditor Files | *.pixi|PNG Files|*.png",
                DefaultExt = "pixi"
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

        private void OpenDocument(string path, bool openOld = false)
        {
            Document document;

            if (openOld)
            {
                document = Importer.ImportOldDocument(path);
            }
            else
            {
                document = Importer.ImportDocument(path);
            }

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
                string.IsNullOrEmpty(Owner.BitmapManager.ActiveDocument.DocumentFilePath) ||
                !Owner.BitmapManager.ActiveDocument.DocumentFilePath.EndsWith(".pixi"))
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
            WriteableBitmap bitmap = Owner.BitmapManager.GetCombinedLayersBitmap();
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
    }
}