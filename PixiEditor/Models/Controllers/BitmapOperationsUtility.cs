using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.Controllers
{
    public class BitmapOperationsUtility : NotifyableObject
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

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;
        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        private Coordinates _lastMousePos;
        private BitmapPixelChanges _lastChangedPixels;

        public BitmapOperationsUtility()
        {
            MouseController = new MouseMovementController();
            MouseController.StartedRecordingChanges += MouseController_StartedRecordingChanges;
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
            MouseController.StoppedRecordingChanges += MouseController_StoppedRecordingChanges;
        }

        public void SetActiveTool(Tool tool)
        {
            if(PreviewLayer != null)
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

        private void MouseController_StartedRecordingChanges(object sender, EventArgs e)
        {
            if (PreviewLayer != null)
            {
                PreviewLayer.Clear();
            }
        }

        private void MouseController_StoppedRecordingChanges(object sender, EventArgs e)
        {
            if(SelectedTool.RequiresPreviewLayer)
            {
                BitmapPixelChanges oldValues = GetOldPixelsValues(_lastChangedPixels.ChangedPixels.Keys.ToArray());
                Layers[ActiveLayerIndex].ApplyPixels(_lastChangedPixels);
                BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(_lastChangedPixels, oldValues, ActiveLayerIndex));
                _previewLayer.Clear();
            }
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            if(SelectedTool != null && SelectedTool.ToolType != ToolType.None && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var mouseMove = MouseController.LastMouseMoveCoordinates.ToList();
                if (Layers.Count == 0 || mouseMove.Count == 0) return;
                mouseMove.Reverse();
                UseTool(mouseMove);
                
                _lastMousePos = e.NewPosition;
            }
            else if(Mouse.LeftButton == MouseButtonState.Released)
            {
                HighlightPixels(e.NewPosition);
            }
        }

        private void HighlightPixels(Coordinates newPosition)
        {
            if (Layers.Count == 0 || SelectedTool.HideHighlight) return;
            GeneratePreviewLayer();
            PreviewLayer.Clear();
            Coordinates[] highlightArea = CoordinatesCalculator.RectangleToCoordinates(CoordinatesCalculator.CalculateThicknessCenter(newPosition, ToolSize));    
            PreviewLayer.ApplyPixels(BitmapPixelChanges.FromSingleColoredArray(highlightArea, Color.FromArgb(77,0,0,0)));
        }

        private void UseTool(List<Coordinates> mouseMoveCords)
        {
            if (SelectedTool.PerformsOperationOnBitmap == false) return;
            if (Keyboard.IsKeyDown(Key.LeftShift) && !MouseCordsNotInLine(mouseMoveCords))
            {
                mouseMoveCords = GetSquareCoordiantes(mouseMoveCords);
            }
            if (!SelectedTool.RequiresPreviewLayer)
            {
                BitmapPixelChanges changedPixels = SelectedTool.Use(Layers[ActiveLayerIndex], mouseMoveCords.ToArray(), PrimaryColor);
                BitmapPixelChanges oldPixelsValues = GetOldPixelsValues(changedPixels.ChangedPixels.Keys.ToArray());
                ActiveLayer.ApplyPixels(changedPixels);
                BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(changedPixels, oldPixelsValues, ActiveLayerIndex));
            }
            else
            {
                UseToolOnPreviewLayer(mouseMoveCords);
            }
        }

        private bool MouseCordsNotInLine(List<Coordinates> cords)
        {
            return cords[0].X == cords[^1].X || cords[0].Y == cords[^1].Y;
        }

        /// <summary>
        /// Extracts square from rectangle mouse drag, used to draw symmetric shapes.
        /// </summary>
        /// <param name="mouseMoveCords"></param>
        /// <returns></returns>
        private List<Coordinates> GetSquareCoordiantes(List<Coordinates> mouseMoveCords)
        {
            int xLength = mouseMoveCords[0].Y - mouseMoveCords[^1].Y;
            int yLength = mouseMoveCords[0].Y - mouseMoveCords[^1].Y;
            if(mouseMoveCords[^1].Y > mouseMoveCords[0].Y)
            {
                xLength *= -1;
            }
            if(mouseMoveCords[^1].X > mouseMoveCords[0].X)
            {
                xLength *= -1;
            }
            mouseMoveCords[0] = new Coordinates(mouseMoveCords[^1].X + xLength, mouseMoveCords[^1].Y + yLength);
            return mouseMoveCords;
        }

        private BitmapPixelChanges GetOldPixelsValues(Coordinates[] coordinates)
        {
            Dictionary<Coordinates, Color> values = new Dictionary<Coordinates, Color>();
            Layers[ActiveLayerIndex].LayerBitmap.Lock();
            for (int i = 0; i < coordinates.Length; i++)
            {
                if (coordinates[i].X < 0 || coordinates[i].X > Layers[0].Width - 1 || coordinates[i].Y < 0 || coordinates[i].Y > Layers[0].Height - 1) 
                    continue;
                values.Add(coordinates[i], Layers[ActiveLayerIndex].LayerBitmap.GetPixel(coordinates[i].X, coordinates[i].Y));
            }
            Layers[ActiveLayerIndex].LayerBitmap.Unlock();
            return new BitmapPixelChanges(values);
        }

        private void UseToolOnPreviewLayer(List<Coordinates> mouseMove)
        {
            BitmapPixelChanges changedPixels;
            if (mouseMove.Count > 0 && mouseMove[0] != _lastMousePos)
            {
                GeneratePreviewLayer();
                PreviewLayer.Clear();
                changedPixels = SelectedTool.Use(Layers[ActiveLayerIndex], mouseMove.ToArray(), PrimaryColor);
                PreviewLayer.ApplyPixels(changedPixels);
                _lastChangedPixels = changedPixels;
            }
        }

        private void GeneratePreviewLayer()
        {
            if (PreviewLayer == null)
            {
                PreviewLayer = new Layer("_previewLayer", Layers[0].Width, Layers[0].Height);
            }
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
            else if(ActiveLayerIndex > Layers.Count - 1)
            {
                SetActiveLayer(Layers.Count - 1);
            }
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

        public void SetActiveLayer(int index)
        {
            if (ActiveLayerIndex <= Layers.Count - 1)
            {
                Layers[ActiveLayerIndex].IsActive = false;
            }
                ActiveLayerIndex = index;
            Layers[ActiveLayerIndex].IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
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
    }
}

public class BitmapChangedEventArgs : EventArgs
{
    public BitmapPixelChanges PixelsChanged { get; set; }
    public BitmapPixelChanges OldPixelsValues { get; set; }
    public int ChangedLayerIndex { get; set; }

    public BitmapChangedEventArgs(BitmapPixelChanges pixelsChanged, BitmapPixelChanges oldPixelsValues, int changedLayerIndex)
    {
        PixelsChanged = pixelsChanged;
        OldPixelsValues = oldPixelsValues;
        ChangedLayerIndex = changedLayerIndex;
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
