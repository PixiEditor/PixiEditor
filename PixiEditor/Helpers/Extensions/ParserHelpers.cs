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
            ObservableCollection<Layer> layers = new();
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
            Parser.SerializableDocument serializable = new Parser.SerializableDocument
            {
                Width = document.Width,
                Height = document.Height,
                Layers = document.Layers.Select(x => x.ToSerializable()).ToArray(),
                Swatches = document.Swatches.Select(x => new Tuple<byte, byte, byte, byte>(x.A, x.R, x.G, x.B)).ToArray()
            };

            return serializable;
        }

        public static Parser.SerializableLayer ToSerializable(this Layer layer)
        {
            Parser.SerializableLayer serializable = new Parser.SerializableLayer
            {
                Name = layer.Name,
                Width = layer.Width,
                Height = layer.Height,
                BitmapBytes = layer.ConvertBitmapToBytes(),
                IsVisible = layer.IsVisible,
                OffsetX = (int)layer.Offset.Left,
                OffsetY = (int)layer.Offset.Top,
                Opacity = layer.Opacity,
                MaxWidth = layer.MaxWidth,
                MaxHeight = layer.MaxHeight
            };

            return serializable;
        }
    }
}