using System;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class SerializableLayer
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] BitmapBytes { get; set; }
        public bool IsVisible { get; set; }

        public SerializableLayer(Layer layer)
        {
            Name = layer.Name;
            Width = layer.Width;
            Height = layer.Height;
            BitmapBytes = layer.ConvertBitmapToBytes();
            IsVisible = layer.IsVisible;
        }
    }
}