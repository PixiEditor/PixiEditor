using System;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;

namespace PixiEditor.Models.Undo
{
    [Serializable]
    public record UndoLayer
    {
        public string StoredPngLayerName { get; set; }

        public Guid LayerGuid { get; init; }

        public string LayerHighlightColor { get; set; }

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

        //public SKRectI SerializedRect { get; set; }

        public UndoLayer(string storedPngLayerName, Layer layer, int layerIndex/*, SKRectI serializedRect*/)
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
            LayerGuid = layer.GuidValue;
            LayerHighlightColor = layer.LayerHighlightColor;
            //SerializedRect = serializedRect;
        }
    }
}