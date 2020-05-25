using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiTools = PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;
using PixiEditor.Models.Position;
using PixiEditor.Models.DataHolders;
using System.Linq;
using System.Collections.ObjectModel;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {

        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand GenerateDrawAreaCommand { get; set; } //Command that generates draw area
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


        private Color _primaryColor = Colors.White;

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

        private Color _secondaryColor = Colors.Black;

        public Color SecondaryColor //Secondary color, hooked with right mouse button
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
                    RaisePropertyChanged("SelectedTool"); } }
        }        

        public ObservableCollection<Tool> ToolSet { get; set; }

        private LayerChanges _undoChanges;

        public LayerChanges UndoChanges
        {
            get { return _undoChanges; }
            set 
            { 
                _undoChanges = value;
                BitmapManager.Layers[value.LayerIndex].ApplyPixels(value.PixelChanges);
            }
        }

        private Cursor _toolCursor;

        public Cursor ToolCursor
        {
            get { return _toolCursor; }
            set {
                _toolCursor = value;
                RaisePropertyChanged("ToolCursor");
            }
        }



        public BitmapManager BitmapManager { get; set; }
        public PixelChangesController ChangesController { get; set; }

        public ShortcutController ShortcutController { get; set; }

        public ViewModelMain()
        {
            PixiFilesManager.InitializeTempDirectories();
            BitmapManager = new BitmapManager();
            BitmapManager.BitmapOperations.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapManager.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            ChangesController = new PixelChangesController();
            SelectToolCommand = new RelayCommand(SetTool);
            GenerateDrawAreaCommand = new RelayCommand(GenerateDrawArea);
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
            ToolSet = new ObservableCollection<Tool> { new PixiTools.PenTool(), new PixiTools.FloodFill(), new PixiTools.LineTool(),
            new PixiTools.CircleTool(), new PixiTools.RectangleTool(), new PixiTools.EarserTool(), new PixiTools.ColorPickerTool(), new PixiTools.BrightnessTool() };
            ShortcutController = new ShortcutController
            {
                Shortcuts = new List<Shortcut> { 
                    new Shortcut(Key.B, SelectToolCommand, ToolType.Pen),
                    new Shortcut(Key.X, SwapColorsCommand),
                    new Shortcut(Key.O, OpenFileCommand, null, ModifierKeys.Control),
                    new Shortcut(Key.E, SelectToolCommand, ToolType.Earser),
                    new Shortcut(Key.O, SelectToolCommand, ToolType.ColorPicker),
                    new Shortcut(Key.R, SelectToolCommand, ToolType.Rectangle),
                    new Shortcut(Key.C, SelectToolCommand, ToolType.Circle),
                    new Shortcut(Key.L, SelectToolCommand, ToolType.Line),
                    new Shortcut(Key.G, SelectToolCommand, ToolType.Bucket),
                    new Shortcut(Key.U, SelectToolCommand, ToolType.Brightness),
                    new Shortcut(Key.Y, RedoCommand, null, ModifierKeys.Control),
                    new Shortcut(Key.Z, UndoCommand),
                    new Shortcut(Key.S, SaveFileCommand, null, ModifierKeys.Control),
                    new Shortcut(Key.N, GenerateDrawAreaCommand, null, ModifierKeys.Control),
                    new Shortcut(Key.S, SaveFileCommand, "AsNew", ModifierKeys.Control | ModifierKeys.Shift)
                }
            };
            UndoManager.SetMainRoot(this);
            SetActiveTool(ToolType.Pen);
            BitmapManager.PrimaryColor = PrimaryColor;
        }

        public void SetTool(object parameter)
        {
            SetActiveTool((ToolType)parameter);
        }

        public void RenameLayer(object parameter)
        {
            BitmapManager.Layers[(int)parameter].IsRenaming = true;
        }

        public void KeyDown(object parameter)
        {
            ShortcutController.KeyPressed(((KeyEventArgs)parameter).Key);
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            if (BitmapManager.SelectedTool is BitmapOperationTool)
            {
                Tuple<LayerChanges, LayerChanges> changes = ChangesController.PopChanges();
                if (changes.Item1.PixelChanges.ChangedPixels.Count > 0)
                    UndoManager.AddUndoChange(new Change("UndoChanges", changes.Item2, changes.Item1)); //Item2 is old value
            }
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(new LayerChanges(e.PixelsChanged, e.ChangedLayerIndex), 
                new LayerChanges(e.OldPixelsValues, e.ChangedLayerIndex));
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
            BitmapManager.Layers.Move(oldIndex, oldIndex + 1);
            if(BitmapManager.ActiveLayerIndex == oldIndex)
            {
                BitmapManager.SetActiveLayer(oldIndex + 1);
            }
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            BitmapManager.Layers.Move(oldIndex, oldIndex - 1);
            if (BitmapManager.ActiveLayerIndex == oldIndex)
            {
                BitmapManager.SetActiveLayer(oldIndex - 1);
            }
        }

        public bool CanMoveToFront(object property)
        {
            return BitmapManager.Layers.Count - 1 > (int)property;
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
            return BitmapManager.Layers.Count > 1;
        }

        #region Undo/Redo
        /// <summary>
        /// Undo last action
        /// </summary>
        /// <param name="parameter"></param>
        public void Undo(object parameter)
        {
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
            if(activeTool != null)
            {
                activeTool.IsActive = false;
            }

            foundTool.IsActive = true;
            BitmapManager.SetActiveTool(foundTool);
            SetToolCursor(tool);
        }

        private void SetToolCursor(ToolType tool)
        {
            if (tool != ToolType.None && tool != ToolType.ColorPicker)
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
            if (BitmapManager.Layers.Count == 0) return;
            if(BitmapManager.SelectedTool.ToolType == ToolType.ColorPicker)
            {
                ExecuteColorPicker();
            }
            else if(Mouse.LeftButton == MouseButtonState.Pressed && BitmapManager.SelectedTool is BitmapOperationTool)
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



        private void ExecuteColorPicker()
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                using (var bitmap = new System.Drawing.Bitmap(1, 1))
                {
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(MousePositionConverter.GetCursorPosition(), new System.Drawing.Point(0, 0), new System.Drawing.Size(1, 1));
                    }
                    var color = bitmap.GetPixel(0, 0);
                    PrimaryColor = Color.FromArgb(color.A, color.R, color.G, color.B);
                }
            }
        }

        /// <summary>
        /// Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        public void GenerateDrawArea(object parameter)
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
            return BitmapManager.ActiveLayer != null;
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
                BitmapManager.ActiveLayer.LayerBitmap = Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight);
            }
        }

        private void NewDocument(int width, int height)
        {
            BitmapManager.Layers.Clear();
            BitmapManager.AddNewLayer("Base Layer", width, height, true);
            BitmapManager.PreviewLayer = null;
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
            BitmapManager.AddNewLayer($"New Layer {BitmapManager.Layers.Count}", BitmapManager.Layers[0].Width, BitmapManager.Layers[0].Height);         
        }

        public bool CanCreateNewLayer(object parameter)
        {
                return BitmapManager.Layers.Count > 0;
        }
    }
}
