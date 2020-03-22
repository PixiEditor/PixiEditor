using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
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
        public Tool SelectedTool { get; set; }

        private ObservableCollection<Layer> _layers;

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

        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public Color PrimaryColor { get; set; }
        
        public int ToolSize { get; set; }

        public event EventHandler<BitmapChangedEventArgs> BitmapChanged;
        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        public BitmapOperationsUtility()
        {
            Layers = new ObservableCollection<Layer>();
            MouseController = new MouseMovementController();
            MouseController.MousePositionChanged += Controller_MousePositionChanged;
        }

        private void Controller_MousePositionChanged(object sender, MouseMovementEventArgs e)
        {
            if(SelectedTool != null && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var pixels = MouseController.LastMouseMoveCoordinates;
                pixels.Reverse();
                var changedPixels = SelectedTool.Use(Layers[ActiveLayerIndex], pixels.ToArray(), PrimaryColor, ToolSize);
                ActiveLayer.ApplyPixels(changedPixels);
                BitmapChanged?.Invoke(this, new BitmapChangedEventArgs(changedPixels, ActiveLayerIndex));
            }
        }

        public void AddNewLayer(string name, int height, int width, bool setAsActive = true)
        {
            Layers.Add(new Layer(name, width, height));
            Layers.Move(Layers.Count - 1, 0);
            if (setAsActive)
            {
                ActiveLayerIndex = 0;
            }
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void SetActiveLayer(int index)
        {
            ActiveLayerIndex = index;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public WriteableBitmap GetCombinedBitmaps()
        {
            Layer[] visibleLayers = Layers.Where(x => x.IsVisible).ToArray();
            visibleLayers.Reverse();
            int width = visibleLayers[0].Width;
            int height = visibleLayers[0].Height;
            WriteableBitmap finalBitmap = BitmapFactory.New(width, height);
            finalBitmap.Lock();
            finalBitmap = WriteLayersToBitmap(finalBitmap, visibleLayers);
            finalBitmap.Unlock();
            return finalBitmap;
        }

        private WriteableBitmap WriteLayersToBitmap(WriteableBitmap targetBitmap, Layer[] layers)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Coordinates[] nonTransparentCords = layers[i].GetNonTransprarentPixels();
                for (int j = 0; j < nonTransparentCords.Length; j++)
                {
                    int x = nonTransparentCords[j].X;
                    int y = nonTransparentCords[j].Y;
                    targetBitmap.SetPixel(x, y, layers[i].LayerBitmap.GetPixel(x,y));
                }
            }
            return targetBitmap;
        }

    }
}

public class BitmapChangedEventArgs : EventArgs
{
    public BitmapPixelChanges PixelsChanged { get; set; }
    public int ChangedLayerIndex { get; set; }

    public BitmapChangedEventArgs(BitmapPixelChanges pixelsChanged, int changedLayerIndex)
    {
        PixelsChanged = pixelsChanged;
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
