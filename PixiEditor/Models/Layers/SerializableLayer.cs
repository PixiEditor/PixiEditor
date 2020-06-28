using System;
using System.Linq;

namespace PixiEditor.Models.Layers
{
    [Serializable]
    public class SerializableLayer
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
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
            MaxWidth = layer.MaxWidth;
            MaxHeight = layer.MaxHeight;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(SerializableLayer)) return false;

            SerializableLayer layer = (SerializableLayer) obj;

            return Equals(layer);
        }

        protected bool Equals(SerializableLayer other)
        {
            return Name == other.Name && Width == other.Width && Height == other.Height && MaxWidth == other.MaxWidth && MaxHeight == other.MaxHeight && BitmapBytes.SequenceEqual(other.BitmapBytes) && IsVisible == other.IsVisible && OffsetX == other.OffsetX && OffsetY == other.OffsetY && Opacity.Equals(other.Opacity);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Name);
            hashCode.Add(Width);
            hashCode.Add(Height);
            hashCode.Add(MaxWidth);
            hashCode.Add(MaxHeight);
            hashCode.Add(BitmapBytes);
            hashCode.Add(IsVisible);
            hashCode.Add(OffsetX);
            hashCode.Add(OffsetY);
            hashCode.Add(Opacity);
            return hashCode.ToHashCode();
        }
    }
}