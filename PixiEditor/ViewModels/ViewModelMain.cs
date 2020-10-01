﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
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

namespace PixiEditor.ViewModels
{
    public class ViewModelMain : ViewModelBase
    {
        private const string ConfirmationDialogMessage = "Document was modified. Do you want to save changes?";

        private Color _primaryColor = Colors.Black;

        private bool _recenterZoombox;

        private Color _secondaryColor = Colors.White;

        private Selection _selection;

        private Cursor _toolCursor;

        private LayerChange[] _undoChanges;

        public Action CloseAction { get; set; }

        public static ViewModelMain Current { get; set; }
        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand OpenNewFilePopupCommand { get; set; } //Command that generates draw area
        public RelayCommand MouseMoveCommand { get; set; } //Command that is used to draw
        public RelayCommand MouseDownCommand { get; set; }
        public RelayCommand KeyDownCommand { get; set; }
        public RelayCommand KeyUpCommand { get; set; }
        public RelayCommand ExportFileCommand { get; set; } //Command that is used to save file
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SetActiveLayerCommand { get; set; }
        public RelayCommand NewLayerCommand { get; set; }
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
        public RelayCommand SelectColorCommand { get; set; }
        public RelayCommand RemoveSwatchCommand { get; set; }
        public RelayCommand SaveDocumentCommand { get; set; }
        public RelayCommand OnStartupCommand { get; set; }
        public RelayCommand CloseWindowCommand { get; set; }
        public RelayCommand CenterContentCommand { get; set; }
        public RelayCommand OpenHyperlinkCommand { get; set; }
        public RelayCommand ZoomCommand { get; set; }
        public RelayCommand ChangeToolSizeCommand { get; set; }


        private double _mouseXonCanvas;

        private double _mouseYonCanvas;

        public double MouseXOnCanvas //Mouse X coordinate relative to canvas
        {
            get => _mouseXonCanvas;
            set
            {
                _mouseXonCanvas = value;
                RaisePropertyChanged("MouseXOnCanvas");
            }
        }

        public double MouseYOnCanvas //Mouse Y coordinate relative to canvas
        {
            get => _mouseYonCanvas;
            set
            {
                _mouseYonCanvas = value;
                RaisePropertyChanged("MouseYOnCanvas");
            }
        }

        public bool RecenterZoombox
        {
            get => _recenterZoombox;
            set
            {
                _recenterZoombox = value;
                RaisePropertyChanged("RecenterZoombox");
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

        public ObservableCollection<Tool> ToolSet { get; set; }

        public LayerChange[] UndoChanges //This acts like UndoManager process, but it was implemented before process system, so it can be transformed into it
        {
            get => _undoChanges;
            set
            {
                _undoChanges = value;
                for (int i = 0; i < value.Length; i++)
                    BitmapManager.ActiveDocument.Layers[value[i].LayerIndex].SetPixels(value[i].PixelChanges);
            }
        }

        public Cursor ToolCursor
        {
            get => _toolCursor;
            set
            {
                _toolCursor = value;
                RaisePropertyChanged("ToolCursor");
            }
        }

        private double _zoomPercentage = 100;

        public double ZoomPercentage
        {
            get { return _zoomPercentage; }
            set 
            {
                _zoomPercentage = value;
                RaisePropertyChanged(nameof(ZoomPercentage));
            }
        }

        public BitmapManager BitmapManager { get; set; }
        public PixelChangesController ChangesController { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public Selection ActiveSelection
        {
            get => _selection;
            set
            {
                _selection = value;
                RaisePropertyChanged("ActiveSelection");
            }
        }

        private bool _restoreToolOnKeyUp = false;
        private Tool _lastActionTool;

        public ViewModelMain()
        {
            BitmapManager = new BitmapManager();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;
            ChangesController = new PixelChangesController();
            SelectToolCommand = new RelayCommand(SetTool, DocumentIsNotNull);
            OpenNewFilePopupCommand = new RelayCommand(OpenNewFilePopup);
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseDownCommand = new RelayCommand(MouseDown);
            ExportFileCommand = new RelayCommand(ExportFile, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            OpenFileCommand = new RelayCommand(Open);
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            DeleteLayerCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            SwapColorsCommand = new RelayCommand(SwapColors);
            KeyDownCommand = new RelayCommand(KeyDown);
            KeyUpCommand = new RelayCommand(KeyUp);
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
            SelectColorCommand = new RelayCommand(SelectColor);
            RemoveSwatchCommand = new RelayCommand(RemoveSwatch);
            SaveDocumentCommand = new RelayCommand(SaveDocument, DocumentIsNotNull);
            OnStartupCommand = new RelayCommand(OnStartup);
            CloseWindowCommand = new RelayCommand(CloseWindow);
            CenterContentCommand = new RelayCommand(CenterContent, DocumentIsNotNull);
            OpenHyperlinkCommand = new RelayCommand(OpenHyperlink);
            ZoomCommand = new RelayCommand(ZoomViewport);
            ChangeToolSizeCommand = new RelayCommand(ChangeToolSize);
            ToolSet = new ObservableCollection<Tool>
            {
                new MoveTool(), new PenTool(), new SelectTool(), new FloodFill(), new LineTool(),
                new CircleTool(), new RectangleTool(), new EraserTool(), new ColorPickerTool(), new BrightnessTool(), 
                new ZoomTool()
            };
            ShortcutController = new ShortcutController
            {
                Shortcuts = new List<Shortcut>
                {
                    //Tools
                    new Shortcut(Key.B, SelectToolCommand, ToolType.Pen),
                    new Shortcut(Key.E, SelectToolCommand, ToolType.Eraser),
                    new Shortcut(Key.O, SelectToolCommand, ToolType.ColorPicker),
                    new Shortcut(Key.R, SelectToolCommand, ToolType.Rectangle),
                    new Shortcut(Key.C, SelectToolCommand, ToolType.Circle),
                    new Shortcut(Key.L, SelectToolCommand, ToolType.Line),
                    new Shortcut(Key.G, SelectToolCommand, ToolType.Bucket),
                    new Shortcut(Key.U, SelectToolCommand, ToolType.Brightness),
                    new Shortcut(Key.V, SelectToolCommand, ToolType.Move),
                    new Shortcut(Key.M, SelectToolCommand, ToolType.Select),
                    new Shortcut(Key.Z, SelectToolCommand, ToolType.Zoom),
                    new Shortcut(Key.OemPlus, ZoomCommand, 115),
                    new Shortcut(Key.OemMinus, ZoomCommand, 85),
                    new Shortcut(Key.OemOpenBrackets, ChangeToolSizeCommand, -1),
                    new Shortcut(Key.OemCloseBrackets, ChangeToolSizeCommand, 1),
                    //Editor
                    new Shortcut(Key.X, SwapColorsCommand),
                    new Shortcut(Key.Y, RedoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Z, UndoCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.D, DeselectCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.A, SelectAllCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.C, CopyCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.V, PasteCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.J, DuplicateCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.X, CutCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.Delete, DeletePixelsCommand),
                    new Shortcut(Key.I, OpenResizePopupCommand, modifier: ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.C, OpenResizePopupCommand, "canvas", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.F11, SystemCommands.MaximizeWindowCommand),
                    //File
                    new Shortcut(Key.O, OpenFileCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, ExportFileCommand,
                        modifier: ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt),
                    new Shortcut(Key.S, SaveDocumentCommand, modifier: ModifierKeys.Control),
                    new Shortcut(Key.S, SaveDocumentCommand, "AsNew", ModifierKeys.Control | ModifierKeys.Shift),
                    new Shortcut(Key.N, OpenNewFilePopupCommand, modifier: ModifierKeys.Control),
                }
            };
            UndoManager.SetMainRoot(this);
            SetActiveTool(ToolType.Move);
            BitmapManager.PrimaryColor = PrimaryColor;
            ActiveSelection = new Selection(Array.Empty<Coordinates>());
            Current = this;
        }

        private void ZoomViewport(object parameter)
        {
            double zoom = (int)parameter;
            ZoomPercentage = zoom;
            ZoomPercentage = 100;
        }

        private void ChangeToolSize(object parameter)
        {
            int increment = (int)parameter;
            int newSize = BitmapManager.ToolSize + increment;
            if (newSize > 0)
            {
                BitmapManager.ToolSize = newSize;
            }
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
            if (BitmapManager.IsActiveDocumentChanged())
            {
                result = ConfirmationDialog.Show(ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes) SaveDocument(null);
            }

            if (result != ConfirmationType.Canceled) ((CancelEventArgs) property).Cancel = false;
        }

        private void OnStartup(object parameter)
        {
            var lastArg = Environment.GetCommandLineArgs().Last();
            if (Importer.IsSupportedFile(lastArg) && File.Exists(lastArg))
                Open(lastArg);
            else
                OpenNewFilePopup(null);
        }

        private void BitmapManager_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            e.NewDocument.DocumentSizeChanged += ActiveDocument_DocumentSizeChanged;
        }

        private void Open(object property)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "All Files|*.*|PixiEditor Files | *.pixi|PNG Files|*.png",
                DefaultExt = "pixi"
            };
            if ((bool) dialog.ShowDialog())
            {
                if (Importer.IsSupportedFile(dialog.FileName))
                    Open(dialog.FileName);
                RecenterZoombox = !RecenterZoombox;
            }
        }

        private void Open(string path)
        {
            if (BitmapManager.IsActiveDocumentChanged())
            {
                var result = ConfirmationDialog.Show(ConfirmationDialogMessage);
                if (result == ConfirmationType.Yes)
                {
                    SaveDocument(null);
                }
                else if (result == ConfirmationType.Canceled)
                {
                    return;
                }
            }

            ResetProgramStateValues();
            if (path.EndsWith(".pixi"))
                OpenDocument(path);
            else
                OpenFile(path);
        }

        private void OpenDocument(string path)
        {
            BitmapManager.ActiveDocument = Importer.ImportDocument(path);
            BitmapManager.LoadedDocument = BitmapManager.ActiveDocument.Clone();
            Exporter.SaveDocumentPath = path;
        }

        private void SaveDocument(object parameter)
        {
            bool paramIsAsNew = parameter != null && parameter.ToString()?.ToLower() == "asnew";
            if (paramIsAsNew || Exporter.SaveDocumentPath == null)
            {
                Exporter.SaveAsEditableFileWithDialog(BitmapManager.ActiveDocument, !paramIsAsNew);
            }
            else
            {
                Exporter.SaveAsEditableFile(BitmapManager.ActiveDocument, Exporter.SaveDocumentPath);
            }
            BitmapManager.LoadedDocument = BitmapManager.ActiveDocument.Clone();
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
            ActiveSelection = new Selection(Array.Empty<Coordinates>());
            RecenterZoombox = !RecenterZoombox;
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
                ActiveSelection.SelectedPoints.ToArray());
        }

        public void ClipCanvas(object parameter)
        {
            BitmapManager.ActiveDocument?.ClipCanvas();
        }

        public void Duplicate(object parameter)
        {
            Copy(null);
            Paste(null);
        }

        public void Cut(object parameter)
        {
            Copy(null);
            BitmapManager.ActiveLayer.SetPixels(
                BitmapPixelChanges.FromSingleColoredArray(ActiveSelection.SelectedPoints.ToArray(),
                    Colors.Transparent));
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
                ActiveSelection.SelectedPoints.ToArray(), BitmapManager.ActiveDocument.Width, BitmapManager.ActiveDocument.Height);
        }

        public void SelectAll(object parameter)
        {
            SelectTool select = new SelectTool();
            ActiveSelection.SetSelection(select.GetAllSelection(), SelectionType.New);
        }

        private bool CanSelectAll(object property)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 0;
        }

        public bool DocumentIsNotNull(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }

        public void Deselect(object parameter)
        {
            ActiveSelection?.Clear();
        }

        private bool SelectionIsNotEmpty(object property)
        {
            return ActiveSelection?.SelectedPoints != null && ActiveSelection.SelectedPoints.Count > 0;
        }

        public void SetTool(object parameter)
        {
            SetActiveTool((ToolType) parameter);
        }

        public void RenameLayer(object parameter)
        {
            BitmapManager.ActiveDocument.Layers[(int) parameter].IsRenaming = true;
        }

        private void KeyUp(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            if (_restoreToolOnKeyUp && ShortcutController.LastShortcut != null && ShortcutController.LastShortcut.ShortcutKey == args.Key)
            {
                _restoreToolOnKeyUp = false;
                SetActiveTool(_lastActionTool);
                ShortcutController.BlockShortcutExecution = false;
            }
        }

        public void KeyDown(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            if (args.IsRepeat && !_restoreToolOnKeyUp && ShortcutController.LastShortcut != null && ShortcutController.LastShortcut.Command == SelectToolCommand)
            {
                _restoreToolOnKeyUp = true;
                ShortcutController.BlockShortcutExecution = true;
            }
            ShortcutController.KeyPressed(args.Key, Keyboard.Modifiers);
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
                    UndoManager.AddUndoChange(new Change("UndoChanges", oldValues, newValues));
                    toolUsed.AfterAddedUndo();
                }
            }
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(new LayerChange(e.PixelsChanged, e.ChangedLayerIndex),
                new LayerChange(e.OldPixelsValues, e.ChangedLayerIndex));
            if (BitmapManager.IsOperationTool())
                AddSwatch(PrimaryColor);
        }

        public void SwapColors(object parameter)
        {
            var tmp = PrimaryColor;
            PrimaryColor = SecondaryColor;
            SecondaryColor = tmp;
        }

        public void MoveLayerToFront(object parameter)
        {
            int oldIndex = (int) parameter;
            BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex + 1);
            if (BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex) BitmapManager.SetActiveLayer(oldIndex + 1);
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int) parameter;
            BitmapManager.ActiveDocument.Layers.Move(oldIndex, oldIndex - 1);
            if (BitmapManager.ActiveDocument.ActiveLayerIndex == oldIndex) BitmapManager.SetActiveLayer(oldIndex - 1);
        }

        public bool CanMoveToFront(object property)
        {
            return DocumentIsNotNull(null) && BitmapManager.ActiveDocument.Layers.Count - 1 > (int) property;
        }

        public bool CanMoveToBack(object property)
        {
            return (int) property > 0;
        }

        public void SetActiveLayer(object parameter)
        {
            BitmapManager.SetActiveLayer((int) parameter);
        }

        public void DeleteLayer(object parameter)
        {
            BitmapManager.RemoveLayer((int) parameter);
        }

        public bool CanDeleteLayer(object property)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 1;
        }

        public void SetActiveTool(ToolType tool)
        {
            Tool foundTool = ToolSet.First(x => x.ToolType == tool);
            SetActiveTool(foundTool);
        }

        public void SetActiveTool(Tool tool)
        {
            Tool activeTool = ToolSet.FirstOrDefault(x => x.IsActive);
            if (activeTool != null) activeTool.IsActive = false;

            tool.IsActive = true;
            _lastActionTool = BitmapManager.SelectedTool;
            BitmapManager.SetActiveTool(tool);
            SetToolCursor(tool.ToolType);
        }

            private void SetToolCursor(ToolType tool)
        {
            if (tool != ToolType.None)
                ToolCursor = BitmapManager.SelectedTool.Cursor;
            else
                ToolCursor = Cursors.Arrow;
        }

        private void MouseDown(object parameter)
        {
            if (BitmapManager.ActiveDocument.Layers.Count == 0) return;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (!BitmapManager.MouseController.IsRecordingChanges)
                {
                    bool clickedOnCanvas = MouseXOnCanvas >= 0 && MouseXOnCanvas <= BitmapManager.ActiveDocument.Width &&
                        MouseYOnCanvas >= 0 && MouseYOnCanvas <= BitmapManager.ActiveDocument.Height;
                    BitmapManager.MouseController.StartRecordingMouseMovementChanges(clickedOnCanvas);
                    BitmapManager.MouseController.RecordMouseMovementChange(MousePositionConverter.CurrentCoordinates);
                }
            }

            // Mouse down is guaranteed to only be raised from within this application, so by subscribing here we
            // only listen for mouse up events that occurred as a result of a mouse down within this application.
            // This seems better than maintaining a global listener indefinitely.
            GlobalMouseHook.OnMouseUp += MouseHook_OnMouseUp;
        }

        // this is public for testing.
        public void MouseHook_OnMouseUp(object sender, Point p)
        {
            GlobalMouseHook.OnMouseUp -= MouseHook_OnMouseUp;
            BitmapManager.MouseController.StopRecordingMouseMovementChanges();
        }

        /// <summary>
        ///     Method connected with command, it executes tool "activity"
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
                BitmapManager.MouseController.MouseMoved(cords);
        }

        /// <summary>
        ///     Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenNewFilePopup(object parameter)
        {
            NewFileDialog newFile = new NewFileDialog();
            if (newFile.ShowDialog()) NewDocument(newFile.Width, newFile.Height);
        }

        /// <summary>
        ///     Opens file from path.
        /// </summary>
        /// <param name="path"></param>
        public void OpenFile(string path)
        {
            ImportFileDialog dialog = new ImportFileDialog();

            if (path != null && File.Exists(path))
                dialog.FilePath = path;

            if (dialog.ShowDialog())
            {
                NewDocument(dialog.FileWidth, dialog.FileHeight, false);
                BitmapManager.AddNewLayer("Image",Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight));
            }
        }

        public void NewDocument(int width, int height, bool addBaseLayer = true)
        {
            BitmapManager.ActiveDocument = new Document(width, height);
            if(addBaseLayer)
                BitmapManager.AddNewLayer("Base Layer");
            ResetProgramStateValues();
            BitmapManager.LoadedDocument = BitmapManager.ActiveDocument.Clone();
        }

        /// <summary>
        ///     Resets most variables and controller, so new documents can be handled.
        /// </summary>
        public void ResetProgramStateValues()
        {
            BitmapManager.PreviewLayer = null;
            UndoManager.UndoStack.Clear();
            UndoManager.RedoStack.Clear();
            ActiveSelection = new Selection(Array.Empty<Coordinates>());
            RecenterZoombox = !RecenterZoombox;
            Exporter.SaveDocumentPath = null;
            BitmapManager.LoadedDocument = null;
        }

        public void NewLayer(object parameter)
        {
            BitmapManager.AddNewLayer($"New Layer {BitmapManager.ActiveDocument.Layers.Count}");
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return BitmapManager.ActiveDocument != null && BitmapManager.ActiveDocument.Layers.Count > 0;
        }

        #region Undo/Redo

        /// <summary>
        ///     Undo last action
        /// </summary>
        /// <param name="parameter"></param>
        public void Undo(object parameter)
        {
            Deselect(null);
            UndoManager.Undo();
        }

        /// <summary>
        ///     Returns true if undo can be done.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanUndo(object property)
        {
            return UndoManager.CanUndo;
        }

        /// <summary>
        ///     Redo last action
        /// </summary>
        /// <param name="parameter"></param>
        public void Redo(object parameter)
        {
            UndoManager.Redo();
        }

        /// <summary>
        ///     Returns true if redo can be done.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanRedo(object property)
        {
            return UndoManager.CanRedo;
        }

        #endregion

        #region SaveFile

        /// <summary>
        ///     Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void ExportFile(object parameter)
        {
            WriteableBitmap bitmap = BitmapManager.GetCombinedLayersBitmap();
            Exporter.Export(bitmap, new Size(bitmap.PixelWidth, bitmap.PixelHeight));
        }

        /// <summary>
        ///     Returns true if file save is possible.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanSave(object property)
        {
            return BitmapManager.ActiveDocument != null;
        }

        #endregion
    }
}