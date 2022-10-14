using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Parser;
using PixiEditor.Parser.Collections;
using PixiEditor.Parser.Skia;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Helpers.Extensions;

internal static class ParserHelpers
{
    public static Image ToImage(this SerializableLayer serializableLayer)
    {
        return Image.FromEncodedData(serializableLayer.PngBytes);
    }
    
    public static DocumentViewModel ToDocument(this SerializableDocument serializableDocument)
    {
        List<SerializableLayer> builtLayers = new List<SerializableLayer>();
        DocumentViewModel vm = DocumentViewModel.Build(builder =>
        {
            builder
                .WithSize(serializableDocument.Width, serializableDocument.Height)
                .WithPalette(serializableDocument.Palette.Select(x => new Color(x.R, x.G, x.B, x.A)).ToList())
                .WithSwatches(serializableDocument.Swatches.Select(x => new Color(x.R, x.G, x.B, x.A)).ToList());

            if (serializableDocument.Groups != null)
            {
                foreach (var group in serializableDocument.Groups)
                {
                    builder.WithFolder((folderBuilder =>
                    {
                        builtLayers = BuildFolder(folderBuilder, group,
                            GatherFolderLayers(group, serializableDocument.Layers),
                            serializableDocument);
                    }));
                }
            }
            
            BuildLayers(serializableDocument.Layers.Where(x => !builtLayers.Contains(x)), builder, serializableDocument);
            SortMembers(builder.Children);
        });

        return vm;
    }

    /// <summary>
    ///     Builds folder and its children.
    /// </summary>
    /// <param name="folderBuilder">Folder to build.</param>
    /// <param name="group">Serialized folder (group), which will be used to build.</param>
    /// <param name="layers">Layers only in this folder.</param>
    /// <param name="doc">Document which contains all the serialized data.</param>
    /// <returns>List of layers which were built.</returns>
    private static List<SerializableLayer> BuildFolder(DocumentViewModelBuilder.FolderBuilder folderBuilder, SerializableGroup group, List<SerializableLayer> layers, SerializableDocument doc)
    {
        List<SerializableLayer> builtLayers = new List<SerializableLayer>(layers);
        folderBuilder
            .WithName(group.Name)
            .WithOpacity(group.Opacity)
            .WithVisibility(group.IsVisible)
            .WithOrderInStructure(group.StartLayer);
            
        folderBuilder.WithChildren((childrenBuilder =>
            {
                if (group.Subgroups != null)
                {
                    foreach (var subGroup in group.Subgroups)
                    {
                        childrenBuilder.WithFolder((subFolderBuilder =>
                        {
                            builtLayers.AddRange(BuildFolder(subFolderBuilder, subGroup,
                                GatherFolderLayers(subGroup, doc.Layers), doc));
                        }));
                    }
                }

                BuildLayers(layers, childrenBuilder, doc);
            }));

        return builtLayers;
    }

    private static void BuildLayers(IEnumerable<SerializableLayer> layers, ChildrenBuilder builder, SerializableDocument document)
    {
        if (layers != null)
        {
            foreach (var layer in layers)
            {
                builder.WithLayer((layerBuilder) =>
                {
                    layerBuilder
                        .WithSize(layer.Width, layer.Height)
                        .WithName(layer.Name)
                        .WithOpacity(layer.Opacity)
                        .WithVisibility(layer.IsVisible)
                        .WithRect(layer.Width, layer.Height, layer.OffsetX, layer.OffsetY)
                        .WithSurface((surfaceBuilder) =>
                        {
                            if (layer.PngBytes is { Length: > 0 })
                            {
                                surfaceBuilder.WithImage(layer.PngBytes);
                            }
                            else
                            {
                                surfaceBuilder.Surface = new Surface(new VecI(1, 1));
                            }
                        })
                        .WithOrderInStructure(document.Layers.IndexOf(layer));
                });
            }
        }
    }
    
    /// <summary>
    ///     Gathers all layers which are in the folder. Excludes layers which are in subfolders.
    /// </summary>
    /// <param name="group">Group which contains folder data.</param>
    /// <param name="serializableDocumentLayers">All layers in document.</param>
    /// <returns>List of layers in folder, excluding layers in nested folders.</returns>
    private static List<SerializableLayer> GatherFolderLayers(SerializableGroup group, LayerCollection serializableDocumentLayers)
    {
        List<SerializableLayer> layers = new List<SerializableLayer>();

        for (int i = group.StartLayer; i <= group.EndLayer; i++)
        {
            layers.Add(serializableDocumentLayers[i]);
        }
        
        if(group.Subgroups is { Count: > 0 })
        {
            foreach (var subGroup in group.Subgroups)
            {
                var nestedGroupLayers = GatherFolderLayers(subGroup, serializableDocumentLayers);
                layers.RemoveAll(x => nestedGroupLayers.Contains(x));
            }
        }

        return layers;
    }
    
    /// <summary>
    /// Sorts StructureMemberBuilder by its OrderInStructure property.
    /// </summary>
    /// <param name="builderChildren">Structure to sort</param>
    private static void SortMembers(List<DocumentViewModelBuilder.StructureMemberBuilder> builderChildren)
    {
        int previousOrder = -1;
        int previousIndex = -1;

        for (var index = 0; index < builderChildren.Count; index++)
        {
            var child = builderChildren[index];
            if (child is DocumentViewModelBuilder.FolderBuilder folderBuilder)
            {
                SortMembers(folderBuilder.Children);
            }

            int order = child.OrderInStructure;
            if (order < previousOrder)
            {
                builderChildren.Remove(child);
                builderChildren.Insert(previousIndex, child);
            }

            previousIndex = builderChildren.IndexOf(child);
            previousOrder = order;
        }
    }

    public static SerializableDocument AddSwatches(this SerializableDocument document, IEnumerable<Color> colors)
    {
        document.Swatches.AddRange(colors.Select(x => System.Drawing.Color.FromArgb(x.A, x.R, x.G, x.B)));
        return document;
    }

    public static SerializableDocument AddPalette(this SerializableDocument document, IEnumerable<Color> palette)
    {
        document.Palette.AddRange(palette.Select(x => System.Drawing.Color.FromArgb(x.A, x.R, x.G, x.B)));
        return document;
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
    }*/
}
