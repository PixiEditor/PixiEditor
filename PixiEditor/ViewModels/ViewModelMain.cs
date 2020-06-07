using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Images;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {
        public static ViewModelMain Current { get; set; } = null;
        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand OpenNewFilePopupCommand { get; set; } //Command that generates draw area
        public RelayCommand MouseMoveCommand { get; set; } //Command that is used to draw
        public RelayCommand MouseDownCommand { get; set; }
        public RelayCommand KeyDownCommand { get; set; }
        public RelayCommand SaveFileCommand { get; set; } //Command that is used to save file
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand MouseUpCommand { get; set; }
        public RelayCommand RecenterZoomboxCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SetActiveLayerCommand { get; set; }
        public RelayCommand NewLayerCommand { get; set; }
        public RelayCommand ReloadImageCommand { get; set; }
        public RelayCommand DeleteLayerCommand { get; set; }
        public RelayCommand RenameLayerCommand { get; set; }
        public RelayCommand MoveToBackCommand { get; set; }
        public RelayCommand MoveToFrontCommand { get; set; }
        public RelayCommand SwapColorsCommand { get; set; }
        public RelayCommand DeselectCommand { get; set; }
        public RelayCommand SelectAllCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand DuplicateCommand { get; set; }
        public RelayCommand CutCommand { get; set; }
        public RelayCommand PasteCommand { get; set; }
        public RelayCommand ClipCanvasCommand { get; set; }
        public RelayCommand DeletePixelsCommand { get; set; }
        public RelayCommand OpenResizePopupCommand { get; set; }


        private double _mouseXonCanvas;

        public double MouseXOnCanvas //Mouse X coordinate relative to canvas
        {
            get => _mouseXonCanvas;
            set { _mouseXonCanvas = value; RaisePropertyChanged("MouseXonCanvas"); }
        }

        private double _mouseYonCanvas;

        public double MouseYOnCanvas //Mouse Y coordinate relative to canvas
        {
            get => _mouseYonCanvas;
            set { _mouseYonCanvas = value; RaisePropertyChanged("MouseYonCanvas"); }
        }


        private Color _primaryColor = Colors.Black;

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

        private Color _secondaryColor = Colors.White;

        public Color SecondaryColor
        {
            get => _secondaryColor;
            set { if (_secondaryColor != value) { _secondaryColor = value; RaisePropertyChanged("SecondaryColor"); } }
        }

        private ToolType _selectedTool;

        public ToolType SelectedTool
        {
            get { return _selectedTool; }
            set
            {
                if (_selectedTool != value)
                {
                    _selectedTool = value;
                    SetActiveTool(value);
                    RaisePropertyChanged("SelectedTool");
                }
            }
        }

        public ObservableCollection<Tool> ToolSet { get; set; }

        private LayerChange[] _undoChanges;

        public LayerChange[] UndoChanges
        {
            get { return _undoChanges; }
            set
            {
                _undoChanges = value;
                for (int i = 0; i < value.Length; i++)
                {
                    BitmapManager.ActiveDocument.Layers[value[i].LayerIndex].ApplyPixels(value[i].PixelChanges);

                }
            }
        }

        private Cursor _toolCursor;

        public Cursor ToolCursor
        {
            get { return _toolCursor; }
            set
            {
                _toolCursor = value;
                RaisePropertyChanged("ToolCursor");
            }
        }

        public BitmapManager BitmapManager { get; set; }
        public PixelChangesController ChangesController { get; set; }

        public ShortcutController ShortcutController { get; set; }
        private Selection _selection = null;

        public Selection ActiveSelection
        {
            get => _selection;
            set
            {
                _selection = value;
                RaisePropertyChanged("ActiveSelection");
            }
        }

        public ClipboardController ClipboardController { get; set; }

        public ViewModelMain()
        {
            FilesManager.InitializeTempDirectories();
            BitmapManager = new BitmapManager();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            ChangesController = new PixelChangesController();
            SelectToolCommand = new RelayCommand(SetTool, DocumentIsNotNull);
            OpenNewFilePopupCommand = new RelayCommand(OpenNewFilePopup);
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseDownCommand = new RelayCommand(MouseDown);
            SaveFileCommand = new RelayCommand(SaveFile, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            MouseUpCommand = new RelayCommand(MouseUp);
            RecenterZoomboxCommand = new RelayCommand(RecenterZoombox);
            OpenFileCommand = new RelayCommand(OpenFile);
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            SwapColorsCommand = new RelayCommand(SwapColors);
            KeyDownCommand = new RelayCommand(KeyDown);
            RenameLayerCommand = new RelayCommand(RenameLayer);
            DeselectCommand = new RelayCommand(Deselect, SelectionIsNotEmpty);
            SelectAllCommand = new RelayCommand(SelectAll, CanSelectAll);
            CopyCommand = new RelayCommand(Copy, SelectionIsNotEmpty);
            DuplicateCommand = new RelayCommand(Duplicate, SelectionIsNotEmpty);
            CutCommand = new RelayCommand(Cut, SelectionIsNotEmpty);
            PasteCommand = new RelayCommand(Paste, CanPaste);
            ClipCanvasCommand = new RelayCommand(ClipCanvas, DocumentIsNotNull);
            DeletePixelsCommand = new RelayCommand(DeletePixels, SelectionIsNotEmpty);
            OpenResizePopupCommand = new RelayCommand(OpenResizePopup, DocumentIsNotNull);
            ToolSet = new ObservableCollection<Tool> {new MoveTool(), new PenTool(), new SelectTool(), new FloodFill(), new LineTool(),
            new CircleTool(), new RectangleTool(), new EarserTool(), new ColorPickerTool(), new BrightnessTool()};
            ShortcutController = new ShortcutController
            {
                Shortcuts = new List<Shortcut> {
                    new Shortcut(Key.B, SelectToolCommand, ToolType.Pen),
                    new Shortcut(Key.X, SwapColorsCommand),
                    new Shortcut(Key.O, OpenFileCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.E, SelectToolCommand, ToolType.Earser),
                    new Shortcut(Key.O, SelectToolCommand, ToolType.ColorPicker),
                    new Shortcut(Key.R, SelectToolCommand, ToolType.Rectangle),
                    new Shortcut(Key.C, SelectToolCommand, ToolType.Circle),
                    new Shortcut(Key.L, SelectToolCommand, ToolType.Line),
                    new Shortcut(Key.G, SelectToolCommand, ToolType.Bucket),
                    new Shortcut(Key.U, SelectToolCommand, ToolType.Brightness),
                    new Shortcut(Key.V, SelectToolCommand, ToolType.Move),
                    new Shortcut(Key.M, SelectToolCommand, ToolType.Select),
                    new Shortcut(Key.Y, RedoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Z, UndoCommand),
                    new Shortcut(Key.S, SaveFileCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.N, OpenNewFilePopupCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, SaveFileCommand, "AsNew", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.D, DeselectCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.A, SelectAllCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.C, CopyCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.V, PasteCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.J, DuplicateCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.X, CutCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Delete, DeletePixelsCommand),
                }
            };
            UndoManager.SetMainRoot(this);
            ClipboardController = new ClipboardController();
            SetActiveTool(ToolType.Move);
            BitmapManager.PrimaryColor = PrimaryColor;
            Current = this;
        }

        private void OpenResizePopup(object parameter)
        {
            ResizeDocumentDialog dialog = new ResizeDocumentDialog();
            dialog.ShowDialog();
            Console.WriteLine();
        }

        private void DeletePixels(object parameter)
        {
            BitmapManager.BitmapOperations.DeletePixels(new Layer[] { BitmapManager.ActiveLayer }, 
                ActiveSelection.SelectedPoints.ToArray());
        }

        public void ClipCanvas(object parameter)
        {
            if (BitmapManager.ActiveDocument != null)
            {
                BitmapManager.ActiveDocument.ClipCanvas();
                ActiveSelection = new Selection(Array.Empty<Coordinates>());
            }
        }

        public void Duplicate(object parameter)
        {
            Copy(null);
            Paste(null);
        }

        public void Cut(object parameter)
        {
            Copy(null);
            BitmapManager.ActiveLayer.
                ApplyPixels(BitmapPixelChanges.FromSingleColoredArray(ActiveSelection.SelectedPoints.ToArray(), Colors.Transparent));
        }

        public void Paste(object parameter)
        {
            ClipboardController.PasteFromClipboard();
        }

        private bool CanPaste(object property)
        {
            return DocumentIsNotNull(null) && ClipboardController.IsImageInClipboard();
        }

        private void Copy(object parameter)
        {
            ClipboardController.CopyToClipboard(BitmapManager.ActiveDocument.Layers.ToArray(), 
                ActiveSelection.SelectedPoints.ToArray());
        }

        public void SelectAll(object parameter)
        {
            SelectTool select = new SelectTool();
            select.Use(select.GetAllSelection());
        }

        private bool CanSelectAll(object property)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 0;
        }

        private bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }

        public void Deselect(object parameter)
        {
            ActiveSelection.Clear();
        }

        private bool SelectionIsNotEmpty(object property)
        {
            return ActiveSelection != null && ActiveSelection.SelectedPoints != null && ActiveSelection.SelectedPoints.Count > 0;
        }

        public void SetTool(object parameter)
        {
            SetActiveTool((ToolType)parameter);
        }

        public void RenameLayer(object parameter)
        {
            BitmapManager.ActiveDocument.Layers[(int)parameter].IsRenaming = true;
        }

        public void KeyDown(object parameter)
        {
            ShortcutController.KeyPressed(((KeyEventArgs)parameter).Key);
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            if (BitmapManager.IsOperationTool(BitmapManager.SelectedTool) 
                && (BitmapManager.SelectedTool as BitmapOperationTool).UseDefaultUndoMethod)
            {
                Tuple<LayerChange, LayerChange>[] changes = ChangesController.PopChanges();
                if (changes != null && changes.Length > 0)
                {
                    LayerChange[] newValues = changes.Select(x => x.Item1).ToArray();
                    LayerChange[] oldValues = changes.Select(x => x.Item2).ToArray();
                    UndoManager.AddUndoChange(new Change("UndoChanges", oldValues, newValues)); 
                    BitmapManager.SelectedTool.AfterAddedUndo();
                }
            }
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(new LayerChange(e.PixelsChanged, e.ChangedLayerIndex),
                new LayerChange(e.OldPixelsValues, e.ChangedLayerIndex));
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }

        public void MoveLayerToFront(object parameter)
        {
            int oldIndex = (int)parameter;
            BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex + 1);
            if (BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex)
            {
                BitmapManager.SetActiveLayer(oldIndex + 1);
            }
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex - 1);
            if (BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex)
            {
                BitmapManager.SetActiveLayer(oldIndex - 1);
            }
        }

        public bool CanMoveToFront(object property)
        {
            return DocumentIsNotNull(null) && BitmapManager.ActiveDocument.Layers.Count - 1 > (int)property;
        }

        public bool CanMoveToBack(object property)
        {
            return (int)property > 0;
        }

        public void SetActiveLayer(object parameter)
        {
            BitmapManager.SetActiveLayer((int)parameter);
        }

        public void DeleteLayer(object parameter)
        {
            BitmapManager.RemoveLayer((int)parameter);
        }

        public bool CanDeleteLayer(object property)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 1;
        }

        #region Undo/Redo
        /// <summary>
        /// Undo last action
        /// </summary>
        /// <param name="parameter"></param>
        public void Undo(object parameter)
        {
            Deselect(null);
            UndoManager.Undo();
        }
        /// <summary>
        /// Returns true if undo can be done.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanUndo(object property)
        {
            return UndoManager.CanUndo;
        }
        /// <summary>
        /// Redo last action
        /// </summary>
        /// <param name="parameter"></param>
        public void Redo(object parameter)
        {
            UndoManager.Redo();
        }
        /// <summary>
        /// Returns true if redo can be done.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanRedo(object property)
        {
            return UndoManager.CanRedo;
        }
        #endregion

        private void SetActiveTool(ToolType tool)
        {
            Tool foundTool = ToolSet.First(x => x.ToolType == tool);
            Tool activeTool = ToolSet.FirstOrDefault(x => x.IsActive);
            if (activeTool != null)
            {
                activeTool.IsActive = false;
            }

            foundTool.IsActive = true;
            BitmapManager.SetActiveTool(foundTool);
            SetToolCursor(tool);
        }

        private void SetToolCursor(ToolType tool)
        {
            if (tool != ToolType.None)
            {
                ToolCursor = BitmapManager.SelectedTool.Cursor;
            }
            else
            {
                ToolCursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// When mouse is up stops recording changes.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUp(object parameter)
        {
            BitmapManager.MouseController.StopRecordingMouseMovementChanges();
        }

        private void MouseDown(object parameter)
        {
            if (BitmapManager.ActiveDocument.Layers.Count == 0) return;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (!BitmapManager.MouseController.IsRecordingChanges)
                {
                    BitmapManager.MouseController.StartRecordingMouseMovementChanges();
                    BitmapManager.MouseController.RecordMouseMovementChange(MousePositionConverter.CurrentCoordinates);
                }
            }
        }

        /// <summary>
        /// Method connected with command, it executes tool "activity"
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseMove(object parameter)
        {
            Coordinates cords = new Coordinates((int)MouseXOnCanvas, (int)MouseYOnCanvas);
            MousePositionConverter.CurrentCoordinates = cords;

            if (BitmapManager.MouseController.IsRecordingChanges && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                BitmapManager.MouseController.RecordMouseMovementChange(cords);
            }
            else
            {
                BitmapManager.MouseController.MouseMoved(cords);
            }
        }

        /// <summary>
        /// Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenNewFilePopup(object parameter)
        {
            NewFileDialog newFile = new NewFileDialog();
            if (newFile.ShowDialog())
            {
                NewDocument(newFile.Width, newFile.Height);
            }
        }
        #region SaveFile
        /// <summary>
        /// Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveFile(object parameter)
        {
            WriteableBitmap bitmap = BitmapManager.GetCombinedLayersBitmap();
            if (Exporter.SavePath == null || (string)parameter == "AsNew")
            {
                Exporter.Export(FileType.PNG, bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
            }
            else
            {
                Exporter.ExportWithoutDialog(FileType.PNG, bitmap);
            }
        }
        /// <summary>
        /// Returns true if file save is possible.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanSave(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }
        #endregion

        /// <summary>
        /// Opens file from path.
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenFile(object parameter)
        {
            ImportFileDialog dialog = new ImportFileDialog();
            if (dialog.ShowDialog())
            {
                NewDocument(dialog.FileWidth, dialog.FileHeight);
                BitmapManager.ActiveDocument.ActiveLayer.LayerBitmap = Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight);
            }
        }

        private void NewDocument(int width, int height)
        {
            BitmapManager.ActiveDocument = new Models.DataHolders.Document(width, height);
            BitmapManager.AddNewLayer("Base Layer", width, height, true);
            BitmapManager.PreviewLayer = null;
            UndoManager.UndoStack.Clear();
            UndoManager.RedoStack.Clear();
            ActiveSelection = new Selection(Array.Empty<Coordinates>());
        }

        /// <summary>
        /// For now, shows not implemented info, lol.
        /// </summary>
        /// <param name="parameter"></param>
        public void RecenterZoombox(object parameter)
        {
            MessageBox.Show("This feature is not implemented yet.", "Feature not implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void NewLayer(object parameter)
        {
            BitmapManager.AddNewLayer($"New Layer {BitmapManager.ActiveDocument.Layers.Count}", BitmapManager.ActiveDocument.Width, BitmapManager.ActiveDocument.Height);
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 0;
        }
    }
}
