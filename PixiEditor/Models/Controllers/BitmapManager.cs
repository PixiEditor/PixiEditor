using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class BitmapManager : NotifyableObject
    {
        public MouseMovementController MouseController { get; set; }
        private Tool _selectedTool;
        public Tool SelectedTool
        {
            get => _selectedTool;
            private set
            {
                _selectedTool = value;
                RaisePropertyChanged("SelectedTool");
            }
        }

        private ObservableCollection<Layer> _layers = new ObservableCollection<Layer>();

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set { if (_layers != value) { _layers = value; } }
        }
        private int _activeLayerIndex;
        public int ActiveLayerIndex
        {
            get => _activeLayerIndex;
            set
            {
                _activeLayerIndex = value;
                RaisePropertyChanged("ActiveLayerIndex");
                RaisePropertyChanged("ActiveLayer");
            }
        }

        private Layer _previewLayer;

        public Layer PreviewLayer
        {
            get { return _previewLayer; }
            set
            {
                _previewLayer = value;
                RaisePropertyChanged("PreviewLayer");
            }
        }


        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public Color PrimaryColor { get; set; }

        public int ToolSize => SelectedTool.Toolbar.GetSetting("ToolSize") != null ? (int)SelectedTool.Toolbar.GetSetting("ToolSize").Value : 1;

        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        public BitmapOperationsUtility BitmapOperations { get; set; }
        public ReadonlyToolUtility ReadonlyToolUtility { get; set; }

        public void SetActiveTool(Tool tool)
        {
            if (PreviewLayer != null)
            {
                PreviewLayer.Clear();
            }
            if (SelectedTool != null)
            {
                SelectedTool.Toolbar.SaveToolbarSettings();
            }
            SelectedTool = tool;
            SelectedTool.Toolbar.LoadSharedSettings();
        }

        public BitmapManager()
        {
            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            BitmapOperations = new BitmapOperationsUtility(this);
            ReadonlyToolUtility = new ReadonlyToolUtility(this);
        }

        public void SetActiveLayer(int index)
        {
            if (ActiveLayerIndex <= Layers.Count - 1)
            {
                ActiveLayer.IsActive = false;
            }
            ActiveLayerIndex = index;
            ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            Layers.Add(new Layer(name, width, height));
            if (setAsActive)
            {
                SetActiveLayer(Layers.Count - 1);
            }
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void RemoveLayer(int layerIndex)
        {
            if (Layers.Count <= 1) return;

            bool wasActive = Layers[layerIndex].IsActive;
            Layers.RemoveAt(layerIndex);
            if (wasActive)
            {
                SetActiveLayer(0);
            }
            else if (ActiveLayerIndex > Layers.Count - 1)
            {
                SetActiveLayer(Layers.Count - 1);
            }
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (IsOperationTool(SelectedTool))
                {
                    BitmapOperations.TriggerAction(e.NewPosition,
                        MouseController.LastMouseMoveCoordinates.ToList(), (BitmapOperationTool)SelectedTool);
                }
                else
                {
                    ReadonlyToolUtility.ExecuteTool((ReadonlyTool)SelectedTool);
                }
            }            
            else if(Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            if (PreviewLayer != null)
            {
                PreviewLayer.Clear();
            }
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            if (IsOperationTool(SelectedTool) && (SelectedTool as BitmapOperationTool).RequiresPreviewLayer)
            {
                BitmapOperations.StopAction();
            }
        }

        public void GeneratePreviewLayer()
        {
            if (PreviewLayer == null)
            {
                PreviewLayer = new Layer("_previewLayer", Layers[0].Width, Layers[0].Height);
            }
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (Layers.Count == 0 || SelectedTool.HideHighlight) return;
            GeneratePreviewLayer();
            PreviewLayer.Clear();
            Coordinates[] highlightArea = CoordinatesCalculator.RectangleToCoordinates(
                CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));
            PreviewLayer.ApplyPixels(BitmapPixelChanges.FromSingleColoredArray(highlightArea, Color.FromArgb(77, 0, 0, 0)));
        }

        public WriteableBitmap GetCombinedLayersBitmap()
        {
            WriteableBitmap finalBitmap = Layers[0].LayerBitmap.Clone();
            finalBitmap.Lock();
            for (int i = 1; i < Layers.Count; i++)
            {
                for (int y = 0; y < finalBitmap.Height; y++)
                {
                    for (int x = 0; x < finalBitmap.Width; x++)
                    {
                        Color color = Layers[i].LayerBitmap.GetPixel(x, y);
                        if (color.A != 0 || color.R != 0 || color.B != 0 || color.G != 0)
                        {
                            finalBitmap.SetPixel(x, y, color);
                        }
                    }
                }
            }
            finalBitmap.Unlock();
            return finalBitmap;
        }


        private bool IsOperationTool(Tool tool)
        {
            return typeof(BitmapOperationTool).IsAssignableFrom(tool.GetType());
        }

    }

    public class LayersChangedEventArgs : EventArgs
    {
        public int LayerAffected { get; set; }
        public LayerAction LayerChangeType { get; set; }

        public LayersChangedEventArgs(int layerAffected, LayerAction layerChangeType)
        {
            LayerAffected = layerAffected;
            LayerChangeType = layerChangeType;
        }
    }
}
