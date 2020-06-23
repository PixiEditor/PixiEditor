using System;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class SerializableLayer
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] BitmapBytes { get; set; }
        public bool IsVisible { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public float Opacity { get; set; }

        public SerializableLayer(Layer layer)
        {
            Name = layer.Name;
            Width = layer.Width;
            Height = layer.Height;
            BitmapBytes = layer.ConvertBitmapToBytes();
            IsVisible = layer.IsVisible;
            OffsetX = (int)layer.Offset.Left;
            OffsetY = (int)layer.Offset.Top;
            Opacity = layer.Opacity;
        }
    }
}