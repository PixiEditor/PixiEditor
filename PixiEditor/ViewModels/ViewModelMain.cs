using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Main;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        private string actionDisplay;
        private bool overrideActionDisplay;

        public static ViewModelMain Current { get; set; }

        public IServiceProvider Services { get; private set; }

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

        public DiscordViewModel DiscordViewModel { get; set; }

        public DebugViewModel DebugSubViewModel { get; set; }

        public BitmapManager BitmapManager { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public StylusViewModel StylusSubViewModel { get; set; }

        public WindowViewModel WindowSubViewModel { get; set; }

        public IPreferences Preferences { get; set; }

        public string ActionDisplay
        {
            get
            {
                if (OverrideActionDisplay)
                {
                    return actionDisplay;
                }

                return ToolsSubViewModel.ActiveTool.ActionDisplay;
            }
            set
            {
                actionDisplay = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a custom action display should be used. If false the action display of the selected tool will be used.
        /// </summary>
        public bool OverrideActionDisplay
        {
            get => overrideActionDisplay;
            set
            {
                SetProperty(ref overrideActionDisplay, value);
                RaisePropertyChanged(nameof(ActionDisplay));
            }
        }

        public bool IsDebug
        {
            get =>
#if DEBUG
                true;
#else
                false;
#endif
        }

        public ViewModelMain(IServiceProvider serviceProvider)
        {
            Current = this;
        }

        public void Setup(IServiceProvider services)
        {
            Services = services;

            Preferences = services.GetRequiredService<IPreferences>();

            Preferences.Init();
            BitmapManager = services.GetRequiredService<BitmapManager>();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;

            SelectionSubViewModel = services.GetService<SelectionViewModel>();

            OnStartupCommand = new RelayCommand(OnStartup);
            CloseWindowCommand = new RelayCommand(CloseWindow);

            FileSubViewModel = services.GetService<FileViewModel>();
            ToolsSubViewModel = services.GetService<ToolsViewModel>();
            ToolsSubViewModel.SelectedToolChanged += BitmapManager_SelectedToolChanged;
            ToolsSubViewModel?.SetupTools(services);

            IoSubViewModel = services.GetService<IoViewModel>();
            LayersSubViewModel = services.GetService<LayersViewModel>();
            ClipboardSubViewModel = services.GetService<ClipboardViewModel>();
            UndoSubViewModel = services.GetService<UndoViewModel>();
            ViewportSubViewModel = services.GetService<ViewportViewModel>();
            ColorsSubViewModel = services.GetService<ColorsViewModel>();
            DocumentSubViewModel = services.GetService<DocumentViewModel>();
            DiscordViewModel = services.GetService<DiscordViewModel>();
            UpdateSubViewModel = services.GetService<UpdateViewModel>();

            WindowSubViewModel = services.GetService<WindowViewModel>();
            StylusSubViewModel = services.GetService<StylusViewModel>();

            AddDebugOnlyViewModels();
            AddReleaseOnlyViewModels();

            ShortcutController = new ShortcutController(
                    new ShortcutGroup(
                        "Tools",
                        CreateToolShortcut<PenTool>(Key.B, "Select Pen Tool"),
                        CreateToolShortcut<EraserTool>(Key.E, "Select Eraser Tool"),
                        CreateToolShortcut<ColorPickerTool>(Key.O, "Select Color Picker Tool"),
                        CreateToolShortcut<RectangleTool>(Key.R, "Select Rectangle Tool"),
                        CreateToolShortcut<CircleTool>(Key.C, "Select Circle Tool"),
                        CreateToolShortcut<LineTool>(Key.L, "Select Line Tool"),
                        CreateToolShortcut<FloodFillTool>(Key.G, "Select Flood Fill Tool"),
                        CreateToolShortcut<BrightnessTool>(Key.U, "Select Brightness Tool"),
                        CreateToolShortcut<MoveTool>(Key.V, "Select Move Tool"),
                        CreateToolShortcut<SelectTool>(Key.M, "Select Select Tool"),
                        CreateToolShortcut<ZoomTool>(Key.Z, "Select Zoom Tool"),
                        CreateToolShortcut<MoveViewportTool>(Key.Space, "Select Viewport Move Tool"),
                        CreateToolShortcut<MagicWandTool>(Key.W, "Select Magic Wand Tool"),
                        new Shortcut(Key.OemPlus, ViewportSubViewModel.ZoomCommand, "Zoom in", 1),
                        new Shortcut(Key.OemMinus, ViewportSubViewModel.ZoomCommand, "Zoom out", -1),
                        new Shortcut(Key.OemOpenBrackets, ToolsSubViewModel.ChangeToolSizeCommand, "Decrease Tool Size", -1),
                        new Shortcut(Key.OemCloseBrackets, ToolsSubViewModel.ChangeToolSizeCommand, "Increase Tool Size", 1)),
                    new ShortcutGroup(
                        "Editor",
                        new Shortcut(Key.X, ColorsSubViewModel.SwapColorsCommand, "Swap primary and secondary color"),
                        new Shortcut(Key.Y, UndoSubViewModel.RedoCommand, "Redo", modifier: ModifierKeys.Control),
                        new Shortcut(Key.Z, UndoSubViewModel.UndoCommand, "Undo", modifier: ModifierKeys.Control),
                        new Shortcut(Key.D, SelectionSubViewModel.DeselectCommand, "Deselect all command", modifier: ModifierKeys.Control),
                        new Shortcut(Key.A, SelectionSubViewModel.SelectAllCommand, "Select all command", modifier: ModifierKeys.Control),
                        new Shortcut(Key.C, ClipboardSubViewModel.CopyCommand, "Copy", modifier: ModifierKeys.Control),
                        new Shortcut(Key.V, ClipboardSubViewModel.PasteCommand, "Paste", modifier: ModifierKeys.Control),
                        new Shortcut(Key.J, ClipboardSubViewModel.DuplicateCommand, "Duplicate", modifier: ModifierKeys.Control),
                        new Shortcut(Key.X, ClipboardSubViewModel.CutCommand, "Cut", modifier: ModifierKeys.Control),
                        new Shortcut(Key.Delete, DocumentSubViewModel.DeletePixelsCommand, "Delete selected pixels"),
                        new Shortcut(Key.I, DocumentSubViewModel.OpenResizePopupCommand, "Resize document", modifier: ModifierKeys.Control | ModifierKeys.Shift),
                        new Shortcut(Key.C, DocumentSubViewModel.OpenResizePopupCommand, "Resize canvas", "canvas", ModifierKeys.Control | ModifierKeys.Shift),
                        new Shortcut(Key.F11, SystemCommands.MaximizeWindowCommand, "Maximize")),
                    new ShortcutGroup(
                        "File",
                        new Shortcut(Key.O, FileSubViewModel.OpenFileCommand, "Open a Document", modifier: ModifierKeys.Control),
                        new Shortcut(Key.S, FileSubViewModel.ExportFileCommand, "Export as image", modifier: ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt),
                        new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, "Save Document", modifier: ModifierKeys.Control),
                        new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, "Save Document As New", "AsNew", ModifierKeys.Control | ModifierKeys.Shift),
                        new Shortcut(Key.N, FileSubViewModel.OpenNewFilePopupCommand, "Create new Document", modifier: ModifierKeys.Control)),
                    new ShortcutGroup(
                        "Layers",
                        new Shortcut(Key.F2, LayersSubViewModel.RenameLayerCommand, "Rename active layer", BitmapManager.ActiveDocument?.ActiveLayerGuid)),
                    new ShortcutGroup(
                        "View",
                        new Shortcut(Key.OemTilde, ViewportSubViewModel.ToggleGridLinesCommand, "Toggle gridlines", modifier: ModifierKeys.Control)));

            MiscSubViewModel = services.GetService<MiscViewModel>();

            // Add F1 shortcut after MiscSubViewModel is constructed
            ShortcutController.ShortcutGroups.Add(
                    new ShortcutGroup(
                        "Misc",
                        new Shortcut(Key.F1, MiscSubViewModel.OpenShortcutWindowCommand, "Open the shortcut window", true)));

            BitmapManager.PrimaryColor = ColorsSubViewModel.PrimaryColor;
        }

        /// <summary>
        ///     Resets most variables and controller, so new documents can be handled.
        /// </summary>
        public void ResetProgramStateValues()
        {
            foreach (var document in BitmapManager.Documents)
            {
                document.PreviewLayer.Reset();
            }
        }

        public bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }
        public void CloseWindow(object property)
        {
            if (!(property is CancelEventArgs))
            {
                throw new ArgumentException();
            }

            ((CancelEventArgs)property).Cancel = !RemoveDocumentsWithSaveConfirmation();
        }

        private void BitmapManager_SelectedToolChanged(object sender, SelectedToolEventArgs e)
        {
            if (e.OldTool != null)
                e.OldTool.PropertyChanged -= SelectedTool_PropertyChanged;
            e.NewTool.PropertyChanged += SelectedTool_PropertyChanged;

            NotifyToolActionDisplayChanged();
            BitmapManager.InputTarget.OnToolChange(e.NewTool);
        }

        private void SelectedTool_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Tool.ActionDisplay))
            {
                NotifyToolActionDisplayChanged();
            }
        }

        private void NotifyToolActionDisplayChanged()
        {
            if (!OverrideActionDisplay) RaisePropertyChanged(nameof(ActionDisplay));
        }

        [Conditional("DEBUG")]
        private void AddDebugOnlyViewModels()
        {
            DebugSubViewModel = new DebugViewModel(this);
        }

        [Conditional("RELEASE")]
        private void AddReleaseOnlyViewModels()
        {
        }

        private Shortcut CreateToolShortcut<T>(Key key, ModifierKeys modifier = ModifierKeys.None)
            where T : Tool
        {
            return new Shortcut(key, ToolsSubViewModel.SelectToolCommand, typeof(T), modifier);
        }

        private Shortcut CreateToolShortcut<T>(Key key, string description, ModifierKeys modifier = ModifierKeys.None)
            where T : Tool
        {
            return new Shortcut(key, ToolsSubViewModel.SelectToolCommand, description, typeof(T), modifier);
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
                    //cancel was pressed in the save file dialog
                    if (!BitmapManager.ActiveDocument.ChangesSaved)
                        return false;
                }
            }

            if (result != ConfirmationType.Canceled)
            {
                var doc = BitmapManager.ActiveDocument;
                BitmapManager.Documents.Remove(doc);
                doc.Dispose();

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
            BitmapManager.ActiveDocument.ChangesSaved = false;
            BitmapManager.ActiveDocument.CenterViewportTrigger.Execute(this, new Size(BitmapManager.ActiveDocument.Width, BitmapManager.ActiveDocument.Height));
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            BitmapManager.ActiveDocument.ChangesSaved = false;
            if (ToolsSubViewModel.ActiveTool is BitmapOperationTool)
            {
                ColorsSubViewModel.AddSwatch(ColorsSubViewModel.PrimaryColor);
            }
        }
    }
}
