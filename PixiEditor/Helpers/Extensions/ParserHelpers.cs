using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PixiEditor.Helpers.Extensions
{
    public static class ParserHelpers
    {
        public static Document ToDocument(this SerializableDocument serializableDocument)
        {
            Document document = new Document(serializableDocument.Width, serializableDocument.Height)
            {
                Layers = serializableDocument.ToLayers(),
                Swatches = new ObservableCollection<SKColor>(serializableDocument.Swatches.ToSKColors())
            };

            document.LayerStructure.Groups = serializableDocument.ToGroups(document);

            if (document.Layers.Count > 0)
            {
                document.SetMainActiveLayer(0);
            }

            return document;
        }

        public static ObservableCollection<Layer> ToLayers(this SerializableDocument document)
        {
            ObservableCollection<Layer> layers = new();

            foreach (SerializableLayer slayer in document)
            {
                layers.Add(slayer.ToLayer());
            }

            return layers;
        }

        public static Layer ToLayer(this SerializableLayer layer)
        {
            return new Layer(layer.Name, new Surface(layer.ToSKImage()))
            {
                Opacity = layer.Opacity,
                IsVisible = layer.IsVisible,
                Offset = new(layer.OffsetX, layer.OffsetY, 0, 0)
            };
        }

        public static ObservableCollection<GuidStructureItem> ToGroups(this SerializableDocument sdocument, Document document)
        {
            ObservableCollection<GuidStructureItem> groups = new();

            if (sdocument.Groups == null)
            {
                return groups;
            }

            foreach (SerializableGroup sgroup in sdocument.Groups)
            {
                groups.Add(sgroup.ToGroup(null, document));
            }

            return groups;
        }

        public static GuidStructureItem ToGroup(this SerializableGroup sgroup, GuidStructureItem parent, Document document)
        {
            GuidStructureItem group = new GuidStructureItem(sgroup.Name, Guid.Empty)
            {
                Opacity = sgroup.Opacity,
                IsVisible = sgroup.IsVisible,
                Parent = parent,
                StartLayerGuid = document.Layers[sgroup.StartLayer].LayerGuid,
                EndLayerGuid = document.Layers[sgroup.EndLayer].LayerGuid
            };

            group.Subgroups = new(sgroup.Subgroups.ToGroups(document, group));

            return group;
        }

        public static SerializableDocument ToSerializable(this Document document)
        {
            return new SerializableDocument(document.Width, document.Height,
                                            document.LayerStructure.Groups.ToSerializable(document),
                                            document.Layers.ToSerializable()).AddSwatches(document.Swatches);
        }

        public static IEnumerable<SerializableLayer> ToSerializable(this IEnumerable<Layer> layers)
        {
            foreach (Layer layer in layers)
            {
                yield return layer.ToSerializable();
            }
        }

        public static SerializableLayer ToSerializable(this Layer layer)
        {
            return new SerializableLayer(layer.Width, layer.Height, layer.OffsetX, layer.OffsetY)
            {
                IsVisible = layer.IsVisible,
                Opacity = layer.Opacity,
                Name = layer.Name
            }.FromSKImage(layer.LayerBitmap.SkiaSurface.Snapshot());
        }

        public static IEnumerable<SerializableGroup> ToSerializable(this IEnumerable<GuidStructureItem> groups, Document document)
        {
            foreach (GuidStructureItem group in groups)
            {
                yield return group.ToSerializable(document);
            }
        }

        public static SerializableGroup ToSerializable(this GuidStructureItem group, Document document)
        {
            SerializableGroup serializable = new SerializableGroup(group.Name, group.Subgroups.ToSerializable(document))
            {
                Opacity = group.Opacity,
                IsVisible = group.IsVisible
            };

            for (int i = 0; i < document.Layers.Count; i++)
            {
                if (group.StartLayerGuid == document.Layers[i].LayerGuid)
                {
                    serializable.StartLayer = i;
                }

                if (group.EndLayerGuid == document.Layers[i].LayerGuid)
                {
                    serializable.EndLayer = i;
                }
            }

            return serializable;
        }

        private static IEnumerable<GuidStructureItem> ToGroups(this IEnumerable<SerializableGroup> groups, Document document, GuidStructureItem parent)
        {
            foreach (SerializableGroup sgroup in groups)
            {
                yield return sgroup.ToGroup(parent, document);
            }
        }

        private static SerializableDocument AddSwatches(this SerializableDocument document, IEnumerable<SKColor> colors)
        {
            document.Swatches.AddRange(colors);
            return document;
        }
    }
}
