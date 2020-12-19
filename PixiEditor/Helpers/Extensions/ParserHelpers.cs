using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;

namespace PixiEditor.Helpers.Extensions
{
    public static class ParserHelpers
    {
        public static Document ToDocument(this Parser.SerializableDocument serializableDocument)
        {
            Document document = new Document(serializableDocument.Width, serializableDocument.Height)
            {
                Layers = serializableDocument.ToLayers(),
                Swatches = new ObservableCollection<Color>(serializableDocument.Swatches.Select(x =>
                    Color.FromArgb(x.Item1, x.Item2, x.Item3, x.Item4)))
            };

            return document;
        }

        public static ObservableCollection<Layer> ToLayers(this Parser.SerializableDocument serializableDocument)
        {
            ObservableCollection<Layer> layers = new ObservableCollection<Layer>();
            for (int i = 0; i < serializableDocument.Layers.Length; i++)
            {
                Parser.SerializableLayer serLayer = serializableDocument.Layers[i];
                Layer layer =
                    new Layer(serLayer.Name, BitmapUtils.BytesToWriteableBitmap(serLayer.Width, serLayer.Height, serLayer.BitmapBytes))
                    {
                        IsVisible = serLayer.IsVisible,
                        Offset = new Thickness(serLayer.OffsetX, serLayer.OffsetY, 0, 0),
                        Opacity = serLayer.Opacity
                    };
                layers.Add(layer);
            }

            return layers;
        }

        public static Parser.SerializableDocument ToSerializable(this Document document)
        {
            Parser.SerializableDocument serializable = new Parser.SerializableDocument();

            serializable.Width = document.Width;
            serializable.Height = document.Height;
            serializable.Layers = document.Layers.Select(x => x.ToSerializable()).ToArray();
            serializable.Swatches = document.Swatches.Select(x => new Tuple<byte, byte, byte, byte>(x.A, x.R, x.G, x.B)).ToArray();

            return serializable;
        }

        public static Parser.SerializableLayer ToSerializable(this Layer layer)
        {
            Parser.SerializableLayer serializable = new Parser.SerializableLayer();

            serializable.Name = layer.Name;
            serializable.Width = layer.Width;
            serializable.Height = layer.Height;
            serializable.BitmapBytes = layer.ConvertBitmapToBytes();
            serializable.IsVisible = layer.IsVisible;
            serializable.OffsetX = (int)layer.Offset.Left;
            serializable.OffsetY = (int)layer.Offset.Top;
            serializable.Opacity = layer.Opacity;
            serializable.MaxWidth = layer.MaxWidth;
            serializable.MaxHeight = layer.MaxHeight;

            return serializable;
        }
    }
}