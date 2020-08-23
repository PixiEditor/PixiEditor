using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers
{
    public class BitmapManager : NotifyableObject
    {
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
            ReadonlyToolUtility = new ReadonlyToolUtility();
        }

        public event EventHandler<LayersChangedEventArgs> LayersChanged;
        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;

        public void SetActiveTool(Tool tool)
        {
            PreviewLayer = null;
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

        public void AddNewLayer(string name, WriteableBitmap bitmap, bool setAsActive = true)
        {
            AddNewLayer(name, bitmap.PixelWidth, bitmap.PixelHeight, setAsActive);
            ActiveDocument.Layers.Last().LayerBitmap = bitmap;
        }

        public void AddNewLayer(string name, bool setAsActive = true)
        {
            AddNewLayer(name, 0, 0, setAsActive);
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            ActiveDocument.Layers.Add(new Layer(name, width, height)
            {
                MaxHeight = ActiveDocument.Height,
                MaxWidth = ActiveDocument.Width
            });
            if (setAsActive) SetActiveLayer(ActiveDocument.Layers.Count - 1);
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void RemoveLayer(int layerIndex)
        {
            if (ActiveDocument.Layers.Count == 0) return;

            bool wasActive = ActiveDocument.Layers[layerIndex].IsActive;
            ActiveDocument.Layers.RemoveAt(layerIndex);
            if (wasActive)
                SetActiveLayer(0);
            else if (ActiveDocument.ActiveLayerIndex > ActiveDocument.Layers.Count - 1)
                SetActiveLayer(ActiveDocument.Layers.Count - 1);
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            SelectedTool.OnMouseMove(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (Mouse.LeftButton == MouseButtonState.Pressed && !IsDraggingViewport() && ActiveDocument != null)
            {
                ExecuteTool(e.NewPosition, MouseController.ClickedOnCanvas);   
            }
            else if (Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        public void ExecuteTool(Coordinates newPosition, bool clickedOnCanvas)
        {
            if (SelectedTool.CanStartOutsideCanvas || clickedOnCanvas)
            {
                if (IsOperationTool(SelectedTool))
                {
                    BitmapOperations.ExecuteTool(newPosition,
                        MouseController.LastMouseMoveCoordinates.ToList(), (BitmapOperationTool)SelectedTool);
                }
                else
                {
                    ReadonlyToolUtility.ExecuteTool(MouseController.LastMouseMoveCoordinates.ToArray(),
                        (ReadonlyTool)SelectedTool);
                }
            }
        }

        private bool IsDraggingViewport()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) && !(SelectedTool is ShapeTool);
        }

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnMouseDown(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            PreviewLayer = null;
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            SelectedTool.OnMouseUp(new MouseEventArgs(Mouse.PrimaryDevice, (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
            if (IsOperationTool(SelectedTool) && ((BitmapOperationTool) SelectedTool).RequiresPreviewLayer)
                BitmapOperations.StopAction();
        }

        public void GeneratePreviewLayer()
        {
            if (ActiveDocument != null)
                PreviewLayer = new Layer("_previewLayer")
                {
                    MaxWidth = ActiveDocument.Width,
                    MaxHeight = ActiveDocument.Height
                };
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (ActiveDocument == null || ActiveDocument.Layers.Count == 0 || SelectedTool.HideHighlight) return;
            Coordinates[] highlightArea = CoordinatesCalculator.RectangleToCoordinates(
                CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));
            if (CanChangeHighlightOffset(highlightArea))
            {
                PreviewLayer.Offset = new Thickness(highlightArea[0].X, highlightArea[0].Y,0,0);
            }
            else if (!IsInsideBounds(highlightArea))
            {
                PreviewLayer = null;
            }
            else
            {
                GeneratePreviewLayer();
                PreviewLayer.SetPixels(
                    BitmapPixelChanges.FromSingleColoredArray(highlightArea, Color.FromArgb(77, 0, 0, 0)));
            }
        }

        private bool CanChangeHighlightOffset(Coordinates[] highlightArea)
        {
            return highlightArea.Length > 0 && PreviewLayer != null && 
                   IsInsideBounds(highlightArea) && highlightArea.Length == PreviewLayer.Width * PreviewLayer.Height;
        }

        private bool IsInsideBounds(Coordinates[] highlightArea)
        {
            return highlightArea[0].X <= ActiveDocument.Width - 1 &&
                    highlightArea[0].Y <= ActiveDocument.Height - 1 &&
                   highlightArea[^1].X >= 0 && highlightArea[^1].Y >= 0;
        }

        public WriteableBitmap GetCombinedLayersBitmap()
        {
            return BitmapUtils.CombineLayers(ActiveDocument.Layers.Where(x => x.IsVisible).ToArray(), ActiveDocument.Width, ActiveDocument.Height);
        }

        /// <summary>
        ///     Returns if selected tool is BitmapOperationTool
        /// </summary>
        /// <returns></returns>
        public bool IsOperationTool()
        {
            return IsOperationTool(SelectedTool);
        }

        /// <summary>
        ///     Returns if tool is BitmapOperationTool
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public static bool IsOperationTool(Tool tool)
        {
            return tool is BitmapOperationTool;
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