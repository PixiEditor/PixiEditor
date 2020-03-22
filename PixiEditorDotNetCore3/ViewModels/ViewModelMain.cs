using PixiEditor.Helpers;
using PixiEditorDotNetCore3.Models.Enums;
using PixiEditorDotNetCore3.Models.Tools;
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
using PixiTools = PixiEditorDotNetCore3.Models.Tools.Tools;
using PixiEditor.Models.Controllers;
using PixiEditorDotNetCore3.Models.Dialogs;
using PixiEditorDotNetCore3.Models.Images;
using PixiEditorDotNetCore3.Models.IO;
using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;
using PixiEditor.Models.Enums;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {

        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand GenerateDrawAreaCommand { get; set; } //Command that generates draw area
        public RelayCommand MouseMoveOrClickCommand { get; set; } //Command that is used to draw
        public RelayCommand SaveFileCommand { get; set; } //Command that is used to save file
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand MouseUpCommand { get; set; }
        public RelayCommand RecenterZoomboxCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }
        public RelayCommand SetActiveLayerCommand { get; set; }
        public RelayCommand NewLayerCommand { get; set; }
        public RelayCommand ReloadImageCommand { get; set; }

        private Image _activeImage;

        public Image ActiveImage
        {
            get => _activeImage;
            set
            {
                _activeImage = value;
                RaisePropertyChanged("ActiveImage");
            }
        }

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

        private ToolsManager primaryToolSet;

        public BitmapOperationsUtility BitmapUtility { get; set; }

        public ViewModelMain()
        {
            PixiFilesManager.InitializeTempDirectories();
            BitmapUtility = new BitmapOperationsUtility();
            BitmapUtility.BitmapChanged += BitmapUtility_BitmapChanged;
            SelectToolCommand = new RelayCommand(RecognizeTool);
            GenerateDrawAreaCommand = new RelayCommand(GenerateDrawArea);
            MouseMoveOrClickCommand = new RelayCommand(MouseMoveOrClick);
            SaveFileCommand = new RelayCommand(SaveFile, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            MouseUpCommand = new RelayCommand(MouseUp);
            RecenterZoomboxCommand = new RelayCommand(RecenterZoombox);
            OpenFileCommand = new RelayCommand(OpenFile);
            SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
            NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
            primaryToolSet = new ToolsManager(new List<Tool> { new PixiTools.PenTool(), new PixiTools.FloodFill(), new PixiTools.LineTool(),
            new PixiTools.CircleTool(), new PixiTools.RectangleTool(), new PixiTools.EarserTool(), new PixiTools.BrightnessTool()});
            UndoManager.SetMainRoot(this);
            SetActiveTool(ToolType.Pen);
            BitmapUtility.PrimaryColor = PrimaryColor;
            ToolSize = 1;
        }

        public void SetActiveLayer(object parameter)
        {
            BitmapUtility.SetActiveLayer((int)parameter);
        }

        private void BitmapUtility_BitmapChanged(object sender, BitmapChangedEventArgs e)
        {
            RefreshImage();
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
            primaryToolSet.SetTool(tool);
            BitmapUtility.SelectedTool = primaryToolSet.SelectedTool;
        }
        /// <summary>
        /// When mouse is up stops recording changes.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUp(object parameter)
        {
            UndoManager.StopRecording();
            BitmapUtility.MouseController.StopRecordingMouseMovementChanges();
            primaryToolSet.StopExectuingTool();
        }

        /// <summary>
        /// Method connected with command, it executes tool "activity"
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseMoveOrClick(object parameter)
        {
            Coordinates cords = new Coordinates((int)MouseXOnCanvas, (int)MouseYOnCanvas);
            MousePositionConverter.CurrentCoordinates = cords;

            if ((Models.Enums.MouseAction)parameter == Models.Enums.MouseAction.MouseDown)
            {
                if (!BitmapUtility.MouseController.IsRecordingChanges)
                {
                    BitmapUtility.MouseController.StartRecordingMouseMovementChanges();
                }
            }
            if((Models.Enums.MouseAction)parameter == Models.Enums.MouseAction.Move)
            {
                if (BitmapUtility.MouseController.IsRecordingChanges)
                {
                    BitmapUtility.MouseController.RecordMouseMovementChanges(cords);
                }
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
                ActiveImage = ImageGenerator.GenerateForPixelArts(newFile.Width, newFile.Height);
            }
        }
        #region SaveFile
        /// <summary>
        /// Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveFile(object parameter)
        {
            if (Exporter.SavePath == null)
            {
                Exporter.Export(FileType.PNG, ActiveImage, new Size(BitmapUtility.ActiveLayer.Width, BitmapUtility.ActiveLayer.Height));
            }
            else
            {
                Exporter.ExportWithoutDialog(FileType.PNG, ActiveImage);
            }
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
                ActiveImage = ImageGenerator.GenerateForPixelArts(dialog.FileWidth, dialog.FileHeight);
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

        public void RefreshImage()
        {
            ActiveImage.Source = BitmapUtility.ActiveLayer.LayerBitmap;
        }

        public void NewLayer(object parameter)
        {
            BitmapUtility.AddNewLayer("New Layer", BitmapUtility.Layers[0].Width, BitmapUtility.Layers[0].Height);
        }

        public bool CanCreateNewLayer(object parameter)
        {
                return BitmapUtility.Layers.Count > 0;
        }
    }
}
