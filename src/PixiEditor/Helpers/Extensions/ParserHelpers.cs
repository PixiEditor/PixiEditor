﻿using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Document;
using PixiEditor.Parser;

namespace PixiEditor.Helpers.Extensions;

internal static class ParserHelpers
{
    public static Document ToDocument(this SerializableDocument serializableDocument)
    {
        /*
        Document document = new Document(serializableDocument.Width, serializableDocument.Height)
        {
            Layers = serializableDocument.ToLayers(),
            Swatches = new WpfObservableRangeCollection<SKColor>(serializableDocument.Swatches.ToSKColors()),
            Palette = new WpfObservableRangeCollection<SKColor>(serializableDocument.Palette.ToSKColors())
        };

        document.LayerStructure.Groups = serializableDocument.ToGroups(document);

        if (document.Layers.Count > 0)
        {
            document.SetMainActiveLayer(0);
        }
        document.Renderer.ForceRerender();

        return document;*/
        throw new NotImplementedException();
    }
    /*
    public static WpfObservableRangeCollection<Layer> ToLayers(this SerializableDocument document)
    {
        WpfObservableRangeCollection<Layer> layers = new();
        foreach (SerializableLayer slayer in document)
        {
            layers.Add(slayer.ToLayer(document.Width, document.Height));
        }

        return layers;
    }

    public static Layer ToLayer(this SerializableLayer layer, int maxWidth, int maxHeight)
    {
        return new Layer(layer.Name, new Surface(layer.ToSKImage()), maxWidth, maxHeight)
        {
            Opacity = layer.Opacity,
            IsVisible = layer.IsVisible,
            Offset = new(layer.OffsetX, layer.OffsetY, 0, 0),
        };
    }
    
    public static WpfObservableRangeCollection<GuidStructureItem> ToGroups(this SerializableDocument sdocument, Document document)
    {
        WpfObservableRangeCollection<GuidStructureItem> groups = new();

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
            StartLayerGuid = document.Layers[sgroup.StartLayer].GuidValue,
            EndLayerGuid = document.Layers[sgroup.EndLayer].GuidValue
        };

        group.Subgroups = new(sgroup.Subgroups.ToGroups(document, group));

        return group;
    }

    public static SerializableDocument ToSerializable(this Document document)
    {
        return new SerializableDocument(document.Width, document.Height,
                document.LayerStructure.Groups.ToSerializable(document),
                document.Layers.ToSerializable())
            .AddSwatches(document.Swatches)
            .AddPalette(document.Palette);
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
            if (group.StartLayerGuid == document.Layers[i].GuidValue)
            {
                serializable.StartLayer = i;
            }

            if (group.EndLayerGuid == document.Layers[i].GuidValue)
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

    private static SerializableDocument AddPalette(this SerializableDocument document, IEnumerable<SKColor> palette)
    {
        document.Palette.AddRange(palette);
        return document;
    }*/
}