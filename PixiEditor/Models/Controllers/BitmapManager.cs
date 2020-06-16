using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.Images;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class BitmapManager : NotifyableObject
    {
        private Document _activeDocument;

        private Layer _previewLayer;
        private Tool _selectedTool;

        public BitmapManager()
        {
            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
            BitmapOperations = new BitmapOperationsUtility(this);
            ReadonlyToolUtility = new ReadonlyToolUtility(this);
        }

        public MouseMovementController MouseController { get; set; }

        public Tool SelectedTool
        {
            get => _selectedTool;
            private set
            {
                _selectedTool = value;
                RaisePropertyChanged("SelectedTool");
            }
        }

        public Layer PreviewLayer
        {
            get => _previewLayer;
            set
            {
                _previewLayer = value;
                RaisePropertyChanged("PreviewLayer");
            }
        }

        public Layer ActiveLayer => ActiveDocument.ActiveLayer;

        public Color PrimaryColor { get; set; }

        public int ToolSize => SelectedTool.Toolbar.GetSetting("ToolSize") != null
            ? (int) SelectedTool.Toolbar.GetSetting("ToolSize").Value
            : 1;

        public BitmapOperationsUtility BitmapOperations { get; set; }
        public ReadonlyToolUtility ReadonlyToolUtility { get; set; }

        public Document ActiveDocument
        {
            get => _activeDocument;
            set
            {
                _activeDocument = value;
                RaisePropertyChanged("ActiveDocument");
                DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(value));
            }
        }

        public event EventHandler<LayersChangedEventArgs> LayersChanged;
        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public void SetActiveTool(Tool tool)
        {
            PreviewLayer?.Clear();
            SelectedTool?.Toolbar.SaveToolbarSettings();
            SelectedTool = tool;
            SelectedTool.Toolbar.LoadSharedSettings();
        }

        public void SetActiveLayer(int index)
        {
            if (ActiveDocument.ActiveLayerIndex <= ActiveDocument.Layers.Count - 1)
                ActiveDocument.ActiveLayer.IsActive = false;
            ActiveDocument.ActiveLayerIndex = index;
            ActiveDocument.ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            ActiveDocument.Layers.Add(new Layer(name, width, height));
            if (setAsActive) SetActiveLayer(ActiveDocument.Layers.Count - 1);
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void RemoveLayer(int layerIndex)
        {
            if (ActiveDocument.Layers.Count <= 1) return;

            bool wasActive = ActiveDocument.Layers[layerIndex].IsActive;
            ActiveDocument.Layers.RemoveAt(layerIndex);
            if (wasActive)
                SetActiveLayer(0);
            else if (ActiveDocument.ActiveLayerIndex > ActiveDocument.Layers.Count - 1)
                SetActiveLayer(ActiveDocument.Layers.Count - 1);
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && ActiveDocument != null)
            {
                if (IsOperationTool(SelectedTool))
                    BitmapOperations.ExecuteTool(e.NewPosition,
                        MouseController.LastMouseMoveCoordinates.ToList(), (BitmapOperationTool) SelectedTool);
                else
                    ReadonlyToolUtility.ExecuteTool(MouseController.LastMouseMoveCoordinates.ToArray(),
                        (ReadonlyTool) SelectedTool);
            }
            else if (Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnMouseDown();
            PreviewLayer?.Clear();
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnMouseUp();
            if (IsOperationTool(SelectedTool) && ((BitmapOperationTool) SelectedTool).RequiresPreviewLayer)
                BitmapOperations.StopAction();
        }

        public void GeneratePreviewLayer()
        {
            if (PreviewLayer == null && ActiveDocument != null || PreviewLayerSizeMismatch())
                PreviewLayer = new Layer("_previewLayer", ActiveDocument.Width, ActiveDocument.Height);
        }

        private bool PreviewLayerSizeMismatch()
        {
            return PreviewLayer.Width != ActiveDocument.Width || PreviewLayer.Height != ActiveDocument.Height;
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (ActiveDocument == null || ActiveDocument.Layers.Count == 0 || SelectedTool.HideHighlight) return;
            GeneratePreviewLayer();
            PreviewLayer.Clear();
            Coordinates[] highlightArea = CoordinatesCalculator.RectangleToCoordinates(
                CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));
            PreviewLayer.ApplyPixels(
                BitmapPixelChanges.FromSingleColoredArray(highlightArea, Color.FromArgb(77, 0, 0, 0)));
        }

        public WriteableBitmap GetCombinedLayersBitmap()
        {
            return BitmapUtils.CombineBitmaps(ActiveDocument.Layers.ToArray());
        }


        public static bool IsOperationTool(Tool tool)
        {
            return tool is BitmapOperationTool;
        }
    }

    public class LayersChangedEventArgs : EventArgs
    {
        public LayersChangedEventArgs(int layerAffected, LayerAction layerChangeType)
        {
            LayerAffected = layerAffected;
            LayerChangeType = layerChangeType;
        }

        public int LayerAffected { get; set; }
        public LayerAction LayerChangeType { get; set; }
    }
}