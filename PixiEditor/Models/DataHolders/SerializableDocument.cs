using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PixiEditor.Models.Images;
using PixiEditor.Models.Layers;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class SerializableDocument
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public SerializableLayer[] Layers { get; set; }
        public Tuple<byte, byte, byte, byte>[] Swatches { get; set; }

        public SerializableDocument(Document document)
        {
            Width = document.Width;
            Height = document.Height;
            Layers = document.Layers.Select(x => new SerializableLayer(x)).ToArray();
            Swatches = document.Swatches.Select(x => new Tuple<byte, byte, byte, byte>(x.A, x.R, x.G, x.B)).ToArray();
        }

        public Document ToDocument()
        {
            Document document = new Document(Width, Height)
            {
                Layers = ToLayers(),
                Swatches = new ObservableCollection<Color>(Swatches.Select(x =>
                    Color.FromArgb(x.Item1, x.Item2, x.Item3, x.Item4)))
            };
            return document;
        }

        private ObservableCollection<Layer> ToLayers()
        {
            ObservableCollection<Layer> layers = new ObservableCollection<Layer>();
            for (int i = 0; i < Layers.Length; i++)
            {
                SerializableLayer serLayer = Layers[i];
                Layer layer =
                    new Layer(BitmapUtils.BytesToWriteableBitmap(serLayer.Width, serLayer.Height, serLayer.BitmapBytes))
                    {
                        IsVisible = serLayer.IsVisible,
                        Name = serLayer.Name,
                        Offset = new Thickness(serLayer.OffsetX, serLayer.OffsetY, 0, 0),
                        Opacity = serLayer.Opacity
                    };
                layers.Add(layer);
            }

            return layers;
        }
    }
}