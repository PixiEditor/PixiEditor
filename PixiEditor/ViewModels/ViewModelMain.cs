using Microsoft.Win32;
using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Tools;
using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.ViewModels
{
    class ViewModelMain : ViewModelBase
    {

        private ObservableCollection<Layer> _layers;

        public ObservableCollection<Layer> Layers
        {
            get { return _layers; }
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

        private Layer _activeLayer;

        public Layer ActiveLayer //Active drawing layer
        {
            get { return _activeLayer; }
            set {
                UndoManager.AddUndoChange("ActiveLayer", _activeLayer, value, "Layer Changed");
                _activeLayer = value;
                RefreshImage();
                RaisePropertyChanged("ActiveLayer");
            }
        }

        private double _mouseXonCanvas;

        public double MouseXOnCanvas //Mouse X coordinate relative to canvas
        {
            get { return _mouseXonCanvas; }
            set { _mouseXonCanvas = value;  RaisePropertyChanged("MouseXonCanvas"); }
        }

        private double _mouseYonCanvas;

        public double MouseYOnCanvas //Mouse Y coordinate relative to canvas
        {
            get { return _mouseYonCanvas; }
            set { _mouseYonCanvas = value; RaisePropertyChanged("MouseYonCanvas"); }
        }


        private Color _primaryColor = Colors.White;

        public Color PrimaryColor //Primary color, hooked with left mouse button
        {
            get { return _primaryColor; }
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
            get { return _secondaryColor; }
            set { if (_secondaryColor != value) { _secondaryColor = value; RaisePropertyChanged("SecondaryColor"); } }
        }
       

        private ToolType _selectedTool = ToolType.Pen;

        public ToolType SelectedTool
        {
            get { return _selectedTool; }
            set { if (_selectedTool != value) { _selectedTool = value; RaisePropertyChanged("SelectedTool"); } }
        }


        private int _toolSize = 1;

        public int ToolSize
        {
            get { return _toolSize; }
            set { if (_toolSize != value) { _toolSize = value; RaisePropertyChanged("ToolSize"); } }
        }

        private ToolSet primaryToolSet;

        public ViewModelMain()
        {
            Layers = new ObservableCollection<Layer>();
            SelectToolCommand = new RelayCommand(RecognizeTool);
            GenerateDrawAreaCommand = new RelayCommand(GenerateDrawArea);
            MouseMoveOrClickCommand = new RelayCommand(MouseMoveOrClick);
            SaveFileCommand = new RelayCommand(SaveFile, CanSave);
            UndoCommand = new RelayCommand(Undo, CanUndo);
            RedoCommand = new RelayCommand(Redo, CanRedo);
            MouseUpCommand = new RelayCommand(MouseUp);
            RecenterZoomboxCommand = new RelayCommand(RecenterZoombox);
            primaryToolSet = new ToolSet();
            UndoManager.SetMainRoot(this);
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
        }

        /// <summary>
        /// Method connected with command, it executes tool "activity"
        /// </summary>
        /// <param name="parameter"></param>
        private void MouseMoveOrClick(object parameter)
        {
            Color color;
            Coordinates cords = new Coordinates((int)MouseXOnCanvas, (int)MouseYOnCanvas);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                color = PrimaryColor;
            }
            else if(Mouse.RightButton == MouseButtonState.Pressed)
            {
                color = SecondaryColor;

            }
            else
            {
                return;
            }

            if (SelectedTool != ToolType.ColorPicker)
            {
                primaryToolSet.UpdateCoordinates(cords);
                primaryToolSet.ExecuteTool(ActiveLayer, cords, color, ToolSize,SelectedTool);
                RefreshImage();
            }
            else
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    PrimaryColor = ToolSet.ColorPicker(ActiveLayer, cords);
                }
                else
                {
                    SecondaryColor = ToolSet.ColorPicker(ActiveLayer, cords);
                }
            }
        }

        private void RefreshImage()
        {
            //Jak nie będzie działać z layerami to tutaj szukaj błędu
            if (ActiveLayer != null)
            {
                Layers[0].LayerImage.Source = ActiveLayer.LayerBitmap;
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
                Layers.Add(new Layer(newFile.Width, newFile.Height));
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
                Exporter.Export(FileType.PNG, ActiveLayer.LayerImage, new Size(ActiveLayer.Width, ActiveLayer.Height));
            }
            else
            {
                Exporter.ExportWithoutDialog(FileType.PNG, ActiveLayer.LayerImage);
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
        /// For now, shows not implemented info, lol.
        /// </summary>
        /// <param name="parameter"></param>
        public void RecenterZoombox(object parameter)
        {
            MessageBox.Show("This feature is not implemented yet.", "Feature not implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
