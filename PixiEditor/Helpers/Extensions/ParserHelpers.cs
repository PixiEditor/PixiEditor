using PixiEditor.Models.DataHolders;
using PixiEditor.Models.ImageManipulation;
using PixiEditor.Models.Layers;
using PixiEditor.Parser;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SDColor = System.Drawing.Color;

namespace PixiEditor.Helpers.Extensions
{
    public static class ParserHelpers
    {
        public static Document ToDocument(this SerializableDocument serializableDocument)
        {
            Document document = new Document(serializableDocument.Width, serializableDocument.Height)
            {
                Layers = serializableDocument.ToLayers(),
                Swatches = new ObservableCollection<Color>(serializableDocument.Swatches.Select(x =>
                    Color.FromArgb(x.A, x.R, x.G, x.B)))
            };

            document.LayerStructure.Groups = serializableDocument.ToGroups();

            if (document.Layers.Count > 0)
            {
                document.SetMainActiveLayer(0);
            }

            return document;
        }

        public static ObservableCollection<GuidStructureItem> ToGroups(this SerializableDocument serializableDocument)
        {
            return ToGroups(serializableDocument.Groups);
        }

        public static ObservableCollection<Layer> ToLayers(this SerializableDocument serializableDocument)
        {
            ObservableCollection<Layer> layers = new ObservableCollection<Layer>();
            for (int i = 0; i < serializableDocument.Layers.Count; i++)
            {
                SerializableLayer serLayer = serializableDocument.Layers[i];
                Layer layer =
                    new Layer(serLayer.Name, BitmapUtils.BytesToWriteableBitmap(serLayer.Width, serLayer.Height, serLayer.BitmapBytes))
                    {
                        IsVisible = serLayer.IsVisible,
                        Offset = new Thickness(serLayer.OffsetX, serLayer.OffsetY, 0, 0),
                        Opacity = serLayer.Opacity,
                        MaxHeight = serializableDocument.Height,
                        MaxWidth = serializableDocument.Width,
                    };
                if (serLayer.LayerGuid != Guid.Empty)
                {
                    layer.ChangeGuid(serLayer.LayerGuid);
                }
                layers.Add(layer);
            }

            return layers;
        }

        public static SerializableDocument ToSerializable(this Document document)
        {
            SerializableDocument serializable = new SerializableDocument
            {
                Width = document.Width,
                Height = document.Height,
                Layers = document.Layers.Select(x => x.ToSerializable()).ToList(),
                Groups = document.LayerStructure.Groups.Select(x => x.ToSerializable()).ToArray(),
                Swatches = document.Swatches.Select(x => SDColor.FromArgb(x.A, x.R, x.G, x.B)).ToList()
            };

            return serializable;
        }

        public static SerializableGuidStructureItem ToSerializable(this GuidStructureItem group, SerializableGuidStructureItem parent = null)
        {
            var serializedGroup = new SerializableGuidStructureItem(
                    group.GroupGuid,
                    group.Name,
                    group.StartLayerGuid,
                    group.EndLayerGuid,
                    null, group.IsVisible, group.Opacity);
            serializedGroup.Subgroups = group.Subgroups.Select(x => x.ToSerializable(serializedGroup)).ToArray();
            return serializedGroup;
        }

        public static SerializableLayer ToSerializable(this Layer layer)
        {
            SerializableLayer serializable = new SerializableLayer
            {
                LayerGuid = layer.LayerGuid,
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

        private static ObservableCollection<GuidStructureItem> ToGroups(SerializableGuidStructureItem[] serializableGroups, GuidStructureItem parent = null)
        {
            ObservableCollection<GuidStructureItem> groups = new ObservableCollection<GuidStructureItem>();

            if (serializableGroups == null)
            {
                return groups;
            }

            foreach (var serializableGroup in serializableGroups)
            {
                groups.Add(ToGroup(serializableGroup, parent));
            }
            return groups;
        }

        private static GuidStructureItem ToGroup(SerializableGuidStructureItem group, GuidStructureItem parent = null)
        {
            if (group == null)
            {
                return null;
            }
            var parsedGroup = new GuidStructureItem(
                group.Name,
                group.StartLayerGuid,
                group.EndLayerGuid,
                new ObservableCollection<GuidStructureItem>(),
                parent)
            { Opacity = group.Opacity, IsVisible = group.IsVisible, GroupGuid = group.GroupGuid, IsExpanded = true };
            parsedGroup.Subgroups = ToGroups(group.Subgroups, parsedGroup);
            return parsedGroup;
        }
    }
}
