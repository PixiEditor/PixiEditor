using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        public static ViewModelMain Current { get; set; }

        public Action CloseAction { get; set; }

        public event EventHandler OnStartupEvent;

        public RelayCommand OnStartupCommand { get; set; }

        public RelayCommand CloseWindowCommand { get; set; }

        public FileViewModel FileSubViewModel { get; set; }

        public UpdateViewModel UpdateSubViewModel { get; set; }

        public ToolsViewModel ToolsSubViewModel { get; set; }

        public IoViewModel IoSubViewModel { get; set; }

        public LayersViewModel LayersSubViewModel { get; set; }

        public ClipboardViewModel ClipboardSubViewModel { get; set; }

        public UndoViewModel UndoSubViewModel { get; set; }

        public SelectionViewModel SelectionSubViewModel { get; set; }

        public ViewportViewModel ViewportSubViewModel { get; set; }

        public ColorsViewModel ColorsSubViewModel { get; set; }

        public DocumentViewModel DocumentSubViewModel { get; set; }

        public MiscViewModel MiscSubViewModel { get; set; }

        public BitmapManager BitmapManager { get; set; }

        public PixelChangesController ChangesController { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public ViewModelMain()
        {
            PreferencesSettings.Init();

            BitmapManager = new BitmapManager();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;

            SelectionSubViewModel = new SelectionViewModel(this);

            ChangesController = new PixelChangesController();
            OnStartupCommand = new RelayCommand(OnStartup);
            CloseWindowCommand = new RelayCommand(CloseWindow);

            FileSubViewModel = new FileViewModel(this);
            UpdateSubViewModel = new UpdateViewModel(this);
            ToolsSubViewModel = new ToolsViewModel(this);
            IoSubViewModel = new IoViewModel(this);
            LayersSubViewModel = new LayersViewModel(this);
            ClipboardSubViewModel = new ClipboardViewModel(this);
            UndoSubViewModel = new UndoViewModel(this);
            ViewportSubViewModel = new ViewportViewModel(this);
            ColorsSubViewModel = new ColorsViewModel(this);
            DocumentSubViewModel = new DocumentViewModel(this);
            MiscSubViewModel = new MiscViewModel(this);

            ShortcutController = new ShortcutController
            {
                Shortcuts = new List<Shortcut>
                {
                    // Tools
                    CreateToolShortcut<PenTool>(Key.B),
                    CreateToolShortcut<EraserTool>(Key.E),
                    CreateToolShortcut<ColorPickerTool>(Key.O),
                    CreateToolShortcut<RectangleTool>(Key.R),
                    CreateToolShortcut<CircleTool>(Key.C),
                    CreateToolShortcut<LineTool>(Key.L),
                    CreateToolShortcut<FloodFill>(Key.G),
                    CreateToolShortcut<BrightnessTool>(Key.U),
                    CreateToolShortcut<MoveTool>(Key.V),
                    CreateToolShortcut<SelectTool>(Key.M),
                    CreateToolShortcut<ZoomTool>(Key.Z),
                    CreateToolShortcut<MoveViewportTool>(Key.H),
                    new Shortcut(Key.OemPlus, ViewportSubViewModel.ZoomCommand, 115),
                    new Shortcut(Key.OemMinus, ViewportSubViewModel.ZoomCommand, 85),
                    new Shortcut(Key.OemOpenBrackets, ToolsSubViewModel.ChangeToolSizeCommand, -1),
                    new Shortcut(Key.OemCloseBrackets, ToolsSubViewModel.ChangeToolSizeCommand, 1),

                    // Editor
                    new Shortcut(Key.X, ColorsSubViewModel.SwapColorsCommand),
                    new Shortcut(Key.Y, UndoSubViewModel.RedoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Z, UndoSubViewModel.UndoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.D, SelectionSubViewModel.DeselectCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.A, SelectionSubViewModel.SelectAllCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.C, ClipboardSubViewModel.CopyCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.V, ClipboardSubViewModel.PasteCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.J, ClipboardSubViewModel.DuplicateCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.X, ClipboardSubViewModel.CutCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Delete, DocumentSubViewModel.DeletePixelsCommand),
                    new Shortcut(Key.I, DocumentSubViewModel.OpenResizePopupCommand, modifier: ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.C, DocumentSubViewModel.OpenResizePopupCommand, "canvas", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.F11, SystemCommands.MaximizeWindowCommand),

                    // File
                    new Shortcut(Key.O, FileSubViewModel.OpenFileCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, FileSubViewModel.ExportFileCommand, modifier: ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt),
                    new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, "AsNew", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.N, FileSubViewModel.OpenNewFilePopupCommand, modifier: ModifierKeys.Control)
                }
            };
            BitmapManager.PrimaryColor = ColorsSubViewModel.PrimaryColor;
            Current = this;
        }

        /// <summary>
        ///     Resets most variables and controller, so new documents can be handled.
        /// </summary>
        public void ResetProgramStateValues()
        {
            foreach (var document in BitmapManager.Documents)
            {
                document.PreviewLayer = null;
            }

            BitmapManager.ActiveDocument?.CenterViewport();
        }

        public bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }

        private Shortcut CreateToolShortcut<T>(Key key, ModifierKeys modifier = ModifierKeys.None)
            where T : Tool
        {
            return new Shortcut(key, ToolsSubViewModel.SelectToolCommand, typeof(T), modifier);
        }

        private void CloseWindow(object property)
        {
            if (!(property is CancelEventArgs))
            {
                throw new ArgumentException();
            }

            ((CancelEventArgs)property).Cancel = !RemoveDocumentsWithSaveConfirmation();
        }

        /// <summary>
        /// Removes documents with unsaved changes confirmation dialog.
        /// </summary>
        /// <returns>If documents was removed successfully.</returns>
        private bool RemoveDocumentsWithSaveConfirmation()
        {
            int docCount = BitmapManager.Documents.Count;
            for (int i = 0; i < docCount; i++)
            {
                BitmapManager.ActiveDocument = BitmapManager.Documents.First();
                bool canceled = !RemoveDocumentWithSaveConfirmation();
                if (canceled)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Removes document with unsaved changes confirmation dialog.
        /// </summary>
        /// <returns>If document was removed successfully.</returns>
        private bool RemoveDocumentWithSaveConfirmation()
        {
            ConfirmationType result = ConfirmationType.No;

            if (!BitmapManager.ActiveDocument.ChangesSaved)
            {
                result = ConfirmationDialog.Show(DocumentViewModel.ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes)
                {
                    FileSubViewModel.SaveDocument(false);
                }
            }

            if (result != ConfirmationType.Canceled)
            {
                BitmapManager.Documents.Remove(BitmapManager.ActiveDocument);

                return true;
            }
            else
            {
                return false;
            }

        }

        private void OnStartup(object parameter)
        {
            OnStartupEvent?.Invoke(this, EventArgs.Empty);
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            if (e.NewDocument != null)
            {
                e.NewDocument.DocumentSizeChanged += ActiveDocument_DocumentSizeChanged;
            }
        }

        private void ActiveDocument_DocumentSizeChanged(object sender, DocumentSizeChangedEventArgs e)
        {
            BitmapManager.ActiveDocument.ActiveSelection = new Selection(Array.Empty<Coordinates>());
            BitmapManager.ActiveDocument.CenterViewport();
            BitmapManager.ActiveDocument.ChangesSaved = false;
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            UndoSubViewModel.TriggerNewUndoChange(BitmapManager.SelectedTool);
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(
                new LayerChange(e.PixelsChanged, e.ChangedLayerIndex),
                new LayerChange(e.OldPixelsValues, e.ChangedLayerIndex));
            BitmapManager.ActiveDocument.ChangesSaved = false;
            if (BitmapManager.IsOperationTool())
            {
                ColorsSubViewModel.AddSwatch(ColorsSubViewModel.PrimaryColor);
            }
        }
    }
}