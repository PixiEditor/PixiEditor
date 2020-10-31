using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        public const string ConfirmationDialogMessage = "Document was modified. Do you want to save changes?";

        public event EventHandler OnStartupEvent;

        private Color _primaryColor = Colors.Black;

        private Color _secondaryColor = Colors.White;

        public bool UnsavedDocumentModified { get; set; }

        public Action CloseAction { get; set; }

        public static ViewModelMain Current { get; set; }                         
        public RelayCommand SwapColorsCommand { get; set; }    
        public RelayCommand ClipCanvasCommand { get; set; }
        public RelayCommand DeletePixelsCommand { get; set; }
        public RelayCommand OpenResizePopupCommand { get; set; }
        public RelayCommand SelectColorCommand { get; set; }
        public RelayCommand RemoveSwatchCommand { get; set; }
        public RelayCommand OnStartupCommand { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }
        public RelayCommand CenterContentCommand { get; set; }
        public RelayCommand OpenHyperlinkCommand { get; set; } 

        public FileViewModel FileSubViewModel { get; set; }
        public UpdateViewModel UpdateSubViewModel { get; set; }
        public ToolsViewModel ToolsSubViewModel { get; set; }
        public IoViewModel IoSubViewModel { get; set; }
        public LayersViewModel LayersSubViewModel { get; set; }
        public ClipboardViewModel ClipboardSubViewModel { get; set; }
        public UndoViewModel UndoSubViewModel { get; set; }
        public SelectionViewModel SelectionSubViewModel { get; set; }
        public ViewportViewModel ViewportSubViewModel { get; set; }


        private double _mouseXonCanvas;

        private double _mouseYonCanvas;

        public double MouseXOnCanvas //Mouse X coordinate relative to canvas
        {
            get => _mouseXonCanvas;
            set
            {
                _mouseXonCanvas = value;
                RaisePropertyChanged(nameof(MouseXOnCanvas));
            }
        }

        public double MouseYOnCanvas //Mouse Y coordinate relative to canvas
        {
            get => _mouseYonCanvas;
            set
            {
                _mouseYonCanvas = value;
                RaisePropertyChanged(nameof(MouseYOnCanvas));
            }
        }

        public Color PrimaryColor //Primary color, hooked with left mouse button
        {
            get => _primaryColor;
            set
            {
                if (_primaryColor != value)
                {
                    _primaryColor = value;
                    BitmapManager.PrimaryColor = value;
                    RaisePropertyChanged("PrimaryColor");
                }
            }
        }

        public Color SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                if (_secondaryColor != value)
                {
                    _secondaryColor = value;
                    RaisePropertyChanged("SecondaryColor");
                }
            }
        }

        public BitmapManager BitmapManager { get; set; }
        public PixelChangesController ChangesController { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public ViewModelMain()
        {
            BitmapManager = new BitmapManager();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;

            SelectionSubViewModel = new SelectionViewModel(this);

            ChangesController = new PixelChangesController();
            SwapColorsCommand = new RelayCommand(SwapColors);           
            ClipCanvasCommand = new RelayCommand(ClipCanvas, DocumentIsNotNull);
            DeletePixelsCommand = new RelayCommand(DeletePixels, SelectionSubViewModel.SelectionIsNotEmpty);
            OpenResizePopupCommand = new RelayCommand(OpenResizePopup, DocumentIsNotNull);
            SelectColorCommand = new RelayCommand(SelectColor);
            RemoveSwatchCommand = new RelayCommand(RemoveSwatch);
            OnStartupCommand = new RelayCommand(OnStartup);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            CenterContentCommand = new RelayCommand(CenterContent, DocumentIsNotNull);
            OpenHyperlinkCommand = new RelayCommand(OpenHyperlink);

            FileSubViewModel = new FileViewModel(this);
            UpdateSubViewModel = new UpdateViewModel(this);
            ToolsSubViewModel = new ToolsViewModel(this);
            IoSubViewModel = new IoViewModel(this);
            LayersSubViewModel = new LayersViewModel(this);
            ClipboardSubViewModel = new ClipboardViewModel(this);
            UndoSubViewModel = new UndoViewModel(this);
            ViewportSubViewModel = new ViewportViewModel(this);


            ShortcutController = new ShortcutController
            {
                Shortcuts = new List<Shortcut>
                {
                    //Tools
                    new Shortcut(Key.B, ToolsSubViewModel.SelectToolCommand, ToolType.Pen),
                    new Shortcut(Key.E, ToolsSubViewModel.SelectToolCommand, ToolType.Eraser),
                    new Shortcut(Key.O, ToolsSubViewModel.SelectToolCommand, ToolType.ColorPicker),
                    new Shortcut(Key.R, ToolsSubViewModel.SelectToolCommand, ToolType.Rectangle),
                    new Shortcut(Key.C, ToolsSubViewModel.SelectToolCommand, ToolType.Circle),
                    new Shortcut(Key.L, ToolsSubViewModel.SelectToolCommand, ToolType.Line),
                    new Shortcut(Key.G, ToolsSubViewModel.SelectToolCommand, ToolType.Bucket),
                    new Shortcut(Key.U, ToolsSubViewModel.SelectToolCommand, ToolType.Brightness),
                    new Shortcut(Key.V, ToolsSubViewModel.SelectToolCommand, ToolType.Move),
                    new Shortcut(Key.M, ToolsSubViewModel.SelectToolCommand, ToolType.Select),
                    new Shortcut(Key.Z, ToolsSubViewModel.SelectToolCommand, ToolType.Zoom),
                    new Shortcut(Key.H, ToolsSubViewModel.SelectToolCommand, ToolType.MoveViewport),
                    new Shortcut(Key.OemPlus, ViewportSubViewModel.ZoomCommand, 115),
                    new Shortcut(Key.OemMinus, ViewportSubViewModel.ZoomCommand, 85),
                    new Shortcut(Key.OemOpenBrackets, ToolsSubViewModel.ChangeToolSizeCommand, -1),
                    new Shortcut(Key.OemCloseBrackets, ToolsSubViewModel.ChangeToolSizeCommand, 1),
                    //Editor
                    new Shortcut(Key.X, SwapColorsCommand),
                    new Shortcut(Key.Y, UndoSubViewModel.RedoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Z,  UndoSubViewModel.UndoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.D, SelectionSubViewModel.DeselectCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.A, SelectionSubViewModel.SelectAllCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.C, ClipboardSubViewModel.CopyCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.V, ClipboardSubViewModel.PasteCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.J, ClipboardSubViewModel.DuplicateCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.X, ClipboardSubViewModel.CutCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Delete, DeletePixelsCommand),
                    new Shortcut(Key.I, OpenResizePopupCommand, modifier: ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.C, OpenResizePopupCommand, "canvas", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.F11, SystemCommands.MaximizeWindowCommand),
                    //File
                    new Shortcut(Key.O, FileSubViewModel.OpenFileCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, FileSubViewModel.ExportFileCommand,
                        modifier: ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt),
                    new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, FileSubViewModel.SaveDocumentCommand, "AsNew", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.N, FileSubViewModel.OpenNewFilePopupCommand, modifier: ModifierKeys.Control),
                }
            };
            UndoManager.SetMainRoot(this);
            BitmapManager.PrimaryColor = PrimaryColor;
            Current = this;
        }        

        private void OpenHyperlink(object parameter)
        {
            if (parameter == null) return;
            string url = (string) parameter;
            var processInfo = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(processInfo);
        }

        private void CenterContent(object property)
        {
            BitmapManager.ActiveDocument.CenterContent();
        }

        private void CloseWindow(object property)
        {
            if (!(property is CancelEventArgs)) throw new ArgumentException();

            ((CancelEventArgs) property).Cancel = true;

            ConfirmationType result = ConfirmationType.No;
            if (UnsavedDocumentModified)
            {
                result = ConfirmationDialog.Show(ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes) 
                {
                    FileSubViewModel.SaveDocument(false); 
                }
            }

            if (result != ConfirmationType.Canceled) ((CancelEventArgs) property).Cancel = false;
        }

        private void OnStartup(object parameter)
        {
            OnStartupEvent?.Invoke(this, EventArgs.Empty);
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.NewDocument.DocumentSizeChanged += ActiveDocument_DocumentSizeChanged;
        }
    

        private void RemoveSwatch(object parameter)
        {
            if (!(parameter is Color)) throw new ArgumentException();
            Color color = (Color) parameter;
            if (BitmapManager.ActiveDocument.Swatches.Contains(color))
                BitmapManager.ActiveDocument.Swatches.Remove(color);
        }

        private void SelectColor(object parameter)
        {
            PrimaryColor = parameter as Color? ?? throw new ArgumentException();
        }

        private void ActiveDocument_DocumentSizeChanged(object sender, DocumentSizeChangedEventArgs e)
        {
            SelectionSubViewModel.ActiveSelection = new Selection(Array.Empty<Coordinates>());
            ViewportSubViewModel.CenterViewport();
            UnsavedDocumentModified = true;
        }

        public void AddSwatch(Color color)
        {
            if (!BitmapManager.ActiveDocument.Swatches.Contains(color))
                BitmapManager.ActiveDocument.Swatches.Add(color);
        }

        private void OpenResizePopup(object parameter)
        {
            bool isCanvasDialog = (string) parameter == "canvas";
            ResizeDocumentDialog dialog = new ResizeDocumentDialog(BitmapManager.ActiveDocument.Width,
                BitmapManager.ActiveDocument.Height, isCanvasDialog);
            if (dialog.ShowDialog())
            {
                if (isCanvasDialog)
                    BitmapManager.ActiveDocument.ResizeCanvas(dialog.Width, dialog.Height, dialog.ResizeAnchor);
                else
                    BitmapManager.ActiveDocument.Resize(dialog.Width, dialog.Height);
            }
        }

        private void DeletePixels(object parameter)
        {
            BitmapManager.BitmapOperations.DeletePixels(new[] {BitmapManager.ActiveLayer},
                SelectionSubViewModel.ActiveSelection.SelectedPoints.ToArray());
        }

        public void ClipCanvas(object parameter)
        {
            BitmapManager.ActiveDocument?.ClipCanvas();
        }        

        public bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }            

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
           TriggerNewUndoChange(BitmapManager.SelectedTool);
        }

        public void TriggerNewUndoChange(Tool toolUsed)
        {
            if (BitmapManager.IsOperationTool(toolUsed)
                && ((BitmapOperationTool) toolUsed).UseDefaultUndoMethod)
            {
                Tuple<LayerChange, LayerChange>[] changes = ChangesController.PopChanges();
                if (changes != null && changes.Length > 0)
                {
                    LayerChange[] newValues = changes.Select(x => x.Item1).ToArray();
                    LayerChange[] oldValues = changes.Select(x => x.Item2).ToArray();
                    UndoManager.AddUndoChange(new Change("UndoChanges", oldValues, newValues, root: UndoSubViewModel));
                    toolUsed.AfterAddedUndo();
                }
            }
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(new LayerChange(e.PixelsChanged, e.ChangedLayerIndex),
                new LayerChange(e.OldPixelsValues, e.ChangedLayerIndex));
            UnsavedDocumentModified = true;
            if (BitmapManager.IsOperationTool())
                AddSwatch(PrimaryColor);
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }             

        /// <summary>
        ///     Resets most variables and controller, so new documents can be handled.
        /// </summary>
        public void ResetProgramStateValues()
        {
            BitmapManager.PreviewLayer = null;
            UndoManager.UndoStack.Clear();
            UndoManager.RedoStack.Clear();
            SelectionSubViewModel.ActiveSelection = new Selection(Array.Empty<Coordinates>());
            ViewportSubViewModel.CenterViewport();
            Exporter.SaveDocumentPath = null;
            UnsavedDocumentModified = false;
        }
    }
}