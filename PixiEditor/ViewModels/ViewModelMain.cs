using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiTools = PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Images;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {

        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand GenerateDrawAreaCommand { get; set; } //Command that generates draw area
        public RelayCommand MouseMoveCommand { get; set; } //Command that is used to draw
        public RelayCommand MouseDownCommand { get; set; }
        public RelayCommand SaveFileCommand { get; set; } //Command that is used to save file
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand MouseUpCommand { get; set; }
        public RelayCommand RecenterZoomboxCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SetActiveLayerCommand { get; set; }
        public RelayCommand NewLayerCommand { get; set; }
        public RelayCommand ReloadImageCommand { get; set; }

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
                    BitmapUtility.PrimaryColor = value;
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


        private int _toolSize;

        public int ToolSize
        {
            get { return _toolSize; }
            set 
            { 
                if (_toolSize != value) 
                { 
                    _toolSize = value;
                    BitmapUtility.ToolSize = value;
                    RaisePropertyChanged("ToolSize"); 
                } 
            }
        }

        public List<Tool> ToolSet { get; set; }

        private LayerChanges _undoChanges;

        public LayerChanges UndoChanges
        {
            get { return _undoChanges; }
            set 
            { 
                _undoChanges = value;
                BitmapUtility.Layers[value.LayerIndex].ApplyPixels(value.PixelChanges);
            }
        }


        public BitmapOperationsUtility BitmapUtility { get; set; }
        public PixelChangesController ChangesController { get; set; }

        public ViewModelMain()
        {
            PixiFilesManager.InitializeTempDirectories();
            BitmapUtility = new BitmapOperationsUtility();
            BitmapUtility.BitmapChanged += BitmapUtility_BitmapChanged;
            BitmapUtility.MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            ChangesController = new PixelChangesController();
            SelectToolCommand = new RelayCommand(RecognizeTool);
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
            ToolSet = new List<Tool> { new PixiTools.PenTool(), new PixiTools.FloodFill(), new PixiTools.LineTool(),
            new PixiTools.CircleTool(), new PixiTools.RectangleTool(), new PixiTools.EarserTool(), new PixiTools.BrightnessTool() };       
            UndoManager.SetMainRoot(this);
            SetActiveTool(ToolType.Pen);
            BitmapUtility.PrimaryColor = PrimaryColor;
            ToolSize = 1;
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            Tuple<LayerChanges, LayerChanges> changes = ChangesController.PopChanges();
            UndoManager.AddUndoChange(new Change("UndoChanges", changes.Item2, changes.Item1)); //Item2 is old value
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            ChangesController.AddChanges(new LayerChanges(e.PixelsChanged, e.ChangedLayerIndex), 
                new LayerChanges(e.OldPixelsValues, e.ChangedLayerIndex));
        }

        public void SetActiveLayer(object parameter)
        {
            BitmapUtility.SetActiveLayer((int)parameter);
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

        /// <summary>
        /// Recognizes selected tool from UI
        /// </summary>
        /// <param name="parameter"></param>
        private void RecognizeTool(object parameter)
        {
            ToolType tool = (ToolType)Enum.Parse(typeof(ToolType), parameter.ToString());
            SelectedTool = tool;
        }

        private void SetActiveTool(ToolType tool)
        {
            BitmapUtility.SelectedTool = ToolSet.Find(x=> x.ToolType == tool);
        }
        /// <summary>
        /// When mouse is up stops recording changes.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUp(object parameter)
        {
            BitmapUtility.MouseController.StopRecordingMouseMovementChanges();
        }

        private void MouseDown(object parameter)
        {
            if (!BitmapUtility.MouseController.IsRecordingChanges)
            {
                BitmapUtility.MouseController.StartRecordingMouseMovementChanges();
                BitmapUtility.MouseController.RecordMouseMovementChange(MousePositionConverter.CurrentCoordinates);
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

                if (BitmapUtility.MouseController.IsRecordingChanges && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    BitmapUtility.MouseController.RecordMouseMovementChange(cords);
                }
        }



        private void ExecuteColorPicker(Coordinates cords)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                PrimaryColor = BitmapUtility.ActiveLayer.LayerBitmap.GetPixel(cords.X, cords.Y);
            }
            else
            {
                SecondaryColor = BitmapUtility.ActiveLayer.LayerBitmap.GetPixel(cords.X, cords.Y);
            }
        }

        /// <summary>
        /// Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        private void GenerateDrawArea(object parameter)
        {
            NewFileDialog newFile = new NewFileDialog();
            if (newFile.ShowDialog())
            {
                BitmapUtility.Layers.Clear();
                BitmapUtility.AddNewLayer("Base Layer", newFile.Width, newFile.Height, true);
            }
        }
        #region SaveFile
        /// <summary>
        /// Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveFile(object parameter)
        {
            //TODO: Blend bitmaps and save file
        //    if (Exporter.SavePath == null)
        //    {
        //        Exporter.Export(FileType.PNG, ActiveImage, new Size(BitmapUtility.ActiveLayer.Width, BitmapUtility.ActiveLayer.Height));
        //    }
        //    else
        //    {
        //        Exporter.ExportWithoutDialog(FileType.PNG, ActiveImage);
        //    }
        }
        /// <summary>
        /// Returns true if file save is possible.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool CanSave(object property)
        {
            return BitmapUtility.ActiveLayer != null;
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
                BitmapUtility.Layers.Clear();
                BitmapUtility.AddNewLayer("Base Layer", dialog.FileWidth, dialog.FileHeight, true);
                BitmapUtility.ActiveLayer.LayerBitmap = Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight);
            }
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
            BitmapUtility.AddNewLayer($"New Layer {BitmapUtility.Layers.Count}", BitmapUtility.Layers[0].Width, BitmapUtility.Layers[0].Height);         
        }

        public bool CanCreateNewLayer(object parameter)
        {
                return BitmapUtility.Layers.Count > 0;
        }
    }
}
