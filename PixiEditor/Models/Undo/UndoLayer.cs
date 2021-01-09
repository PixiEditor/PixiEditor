using PixiEditor.Models.Layers;
using System;

namespace PixiEditor.Models.Undo
{
    [Serializable]
    public record UndoLayer
    {
        public string StoredPngLayerName { get; set; }

        public string Name { get; set; }

        public int LayerIndex { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int MaxWidth { get; set; }

        public int MaxHeight { get; set; }

        public bool IsVisible { get; set; }
        public bool IsActive { get; set; }

        public int OffsetX { get; set; }

        public int OffsetY { get; set; }

        public float Opacity { get; set; }

        public UndoLayer(string storedPngLayerName, Layer layer, int layerIndex)
        {
            StoredPngLayerName = storedPngLayerName;
            LayerIndex = layerIndex;
            Name = layer.Name;
            Width = layer.Width;
            Height = layer.Height;
            MaxWidth = layer.MaxWidth;
            MaxHeight = layer.MaxHeight;
            IsVisible = layer.IsVisible;
            OffsetX = layer.OffsetX;
            OffsetY = layer.OffsetY;
            Opacity = layer.Opacity;
            IsActive = layer.IsActive;
        }
    }
}