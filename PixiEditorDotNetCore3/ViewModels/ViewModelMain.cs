using PixiEditor.Helpers;
using PixiEditorDotNetCore3.Models.Enums;
using PixiEditorDotNetCore3.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiTools = PixiEditorDotNetCore3.Models.Tools.Tools;
using PixiEditorDotNetCore3.Models.Controllers;
using PixiEditorDotNetCore3.Models.Dialogs;
using PixiEditorDotNetCore3.Models.Images;
using PixiEditorDotNetCore3.Models.IO;
using PixiEditorDotNetCore3.Models.Layers;
using PixiEditorDotNetCore3.Models.Position;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {

        private ObservableCollection<Layer> _layers;

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set { if (_layers != value) { _layers = value;} }
        }

        public RelayCommand SelectToolCommand { get; set; } //Command that handles tool switching 
        public RelayCommand GenerateDrawAreaCommand { get; set; } //Command that generates draw area
        public RelayCommand MouseMoveOrClickCommand { get; set; } //Command that is used to draw
        public RelayCommand SaveFileCommand { get; set; } //Command that is used to save file
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand MouseUpCommand { get; set; }
        public RelayCommand RecenterZoomboxCommand { get; set; }
        public RelayCommand OpenFileCommand { get; set; }

        private Image _activeImage;

        public Image ActiveImage
        {
            get => _activeImage;
            set
            {
                _activeImage = BuildFinalImage(value);
                RaisePropertyChanged("ActiveImage");
            }
        }

        private Layer _activeLayer;

        public Layer ActiveLayer //Active drawing layer
        {
            get => _activeLayer;
            set {
                _activeLayer = value;
                RefreshImage();
                RaisePropertyChanged("ActiveLayer");
            }
        }

        public LightLayer ActiveLightLayer
        {
            get
            {
                if (_activeLayer != null)
                    return new LightLayer(
                        _activeLayer.ConvertBitmapToBytes(), 
                        ActiveLayer.Height, ActiveLayer.Width);
                return null;
            }
            set => ActiveLayer = new Layer(BitmapConverter.BytesToWriteableBitmap(ActiveLayer.Width, ActiveLayer.Height,value.LayerBytes));
        }

        private double _mouseXonCanvas;

        public double MouseXOnCanvas //Mouse X coordinate relative to canvas
        {
            get => _mouseXonCanvas;
            set { _mouseXonCanvas = value;  RaisePropertyChanged("MouseXonCanvas"); }
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

        private Color _selectedColor = Colors.White;

        public Color SelectedColor
        {
            get { return _selectedColor; }
            set 
            { 
                if(_selectedColor != value) 
                    _selectedColor = value;
                RaisePropertyChanged("SelectedColor");
            }
        }



        private ToolType _selectedTool = ToolType.Pen;

        public ToolType SelectedTool
        {
            get { return _selectedTool; }
            set { if (_selectedTool != value) { _selectedTool = value; primaryToolSet.SetTool(SelectedTool); RaisePropertyChanged("SelectedTool"); } }
        }


        private int _toolSize = 1;

        public int ToolSize
        {
            get { return _toolSize; }
            set { if (_toolSize != value) { _toolSize = value; RaisePropertyChanged("ToolSize"); } }
        }

        private ToolsManager primaryToolSet;

        public ViewModelMain()
        {
            PixiFilesManager.InitializeTempDirectories();
            Layers = new ObservableCollection<Layer>();
            SelectToolCommand = new RelayCommand(RecognizeTool);
            GenerateDrawAreaCommand = new RelayCommand(GenerateDrawArea);
            MouseMoveOrClickCommand = new RelayCommand(MouseMoveOrClick);
            SaveFileCommand = new RelayCommand(SaveFile, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            MouseUpCommand = new RelayCommand(MouseUp);
            RecenterZoomboxCommand = new RelayCommand(RecenterZoombox);
            OpenFileCommand = new RelayCommand(OpenFile);
            primaryToolSet = new ToolsManager(new List<Tool> { new PixiTools.PenTool(), new PixiTools.FloodFill(), new PixiTools.LineTool(),
            new PixiTools.CircleTool(), new PixiTools.RectangleTool(), new PixiTools.EarserTool(), new PixiTools.BrightnessTool()});
            UndoManager.SetMainRoot(this);
            primaryToolSet.SetTool(SelectedTool);
        }

        public Image BuildFinalImage(Image image)
        {
            if (ActiveLayer == null) return image;
            WriteableBitmap bitmap = BlendLayersBitmaps();
            Image finalImage = image;
            image.Source = bitmap;
            return finalImage;
        }

        public WriteableBitmap BlendLayersBitmaps()
        {
            Rect size = new Rect(new Size(ActiveLayer.Width, ActiveLayer.Height));
            WriteableBitmap bitmap = Layers[0].LayerBitmap;
            for (int i = 1; i < Layers.Count; i++)
            {
                bitmap.Blit(size, Layers[i].LayerBitmap,
                    size, WriteableBitmapExtensions.BlendMode.Additive);
            }

            return bitmap;
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
        /// <summary>
        /// When mouse is up stops recording changes.
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseUp(object parameter)
        {
            UndoManager.StopRecording();
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

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                SelectedColor = PrimaryColor;
            }
            else if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                SelectedColor = SecondaryColor;
            }
            else
            {
                return;
            }

            if (SelectedTool != ToolType.ColorPicker)
            {
                UndoManager.RecordChanges("ActiveLightLayer", new LightLayer(ActiveLayer.ConvertBitmapToBytes(), (int)ActiveLayer.LayerBitmap.Height, 
                    (int)ActiveLayer.LayerBitmap.Width), $"Used {SelectedTool.ToString()}");
                primaryToolSet.ExecuteTool(ActiveLayer, cords, SelectedColor, ToolSize);
                RefreshImage();
            }
            else
            {
                ExecuteColorPicker(cords);
            }
        }

        private void ExecuteColorPicker(Coordinates cords)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                PrimaryColor = ActiveLayer.LayerBitmap.GetPixel(cords.X, cords.Y);
            }
            else
            {
                SecondaryColor = ActiveLayer.LayerBitmap.GetPixel(cords.X, cords.Y);
            }
        }

        private void RefreshImage()
        {
            //If it won't work with layers, bug may occur here
            if (ActiveLayer != null)
            {
                Image activeImage = ActiveImage;
                activeImage.Source = ActiveLayer.LayerBitmap;
                ActiveImage = activeImage;
            }
        }

        /// <summary>
        /// Generates new Layer and sets it as active one
        /// </summary>
        /// <param name="parameter"></param>
        private void GenerateDrawArea(object parameter)
        {
            NewFileDialog newFile = new NewFileDialog();
            if (newFile.ShowDialog() == true)
            {
                Layers.Clear();
                Layers.Add(new Layer("Base Layer",newFile.Width, newFile.Height));
                ActiveImage = ImageGenerator.GenerateForPixelArts(newFile.Width, newFile.Height);
                ActiveLayer = Layers[0];
            }
        }
        #region SaveFile
        /// <summary>
        /// Generates export dialog or saves directly if save data is known.
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveFile(object parameter)
        {
            if (Exporter._savePath == null)
            {
                Exporter.Export(FileType.PNG, ActiveImage, new Size(ActiveLayer.Width, ActiveLayer.Height));
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
            return ActiveLayer != null;
        }
        #endregion

        /// <summary>
        /// Opens file from path.
        /// </summary>
        /// <param name="parameter"></param>
        public void OpenFile(object parameter)
        {
            ImportFileDialog dialog = new ImportFileDialog();
            if (dialog.ShowDialog() == true)
            {
                Layers.Clear();
                Layers.Add(new Layer("Base Layer",dialog.FileWidth, dialog.FileHeight));
                ActiveImage = ImageGenerator.GenerateForPixelArts(dialog.FileWidth, dialog.FileHeight);
                ActiveLayer = Layers[0];
                ActiveLayer.LayerBitmap = Importer.ImportImage(dialog.FilePath, dialog.FileWidth, dialog.FileHeight);
                RefreshImage();
            }
        }

        /// <summary>
        /// For now, shows not implemented info, lol.
        /// </summary>
        /// <param name="parameter"></param>
        public void RecenterZoombox(object parameter)
        {
            Layer testLayer = new Layer("Test Layer", Layers[0].Width, Layers[0].Height);
            testLayer.LayerBitmap.SetPixel(5, 5, Colors.Black);
            Layers.Add(testLayer);
            RefreshImage();

            MessageBox.Show("This feature is not implemented yet.", "Feature not implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
