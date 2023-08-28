using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Parser;
using PixiEditor.Parser.Collections.Deprecated;
using PixiEditor.Parser.Deprecated;

namespace PixiEditor.AvaloniaUI.Helpers;

internal static class SerializableDocumentEx
{
    public static Vector2 ToVector2(this VecD serializableVector2)
    {
        return new Vector2 { X = serializableVector2.X, Y = serializableVector2.Y };
    }
    public static Image ToImage(this SerializableLayer serializableLayer)
    {
        if (serializableLayer.PngBytes == null)
        {
            return null;
        }

        return Image.FromEncodedData(serializableLayer.PngBytes);
    }

    public static DocumentViewModel ToDocument(this SerializableDocument serializableDocument)
    {
        List<SerializableLayer> builtLayers = new List<SerializableLayer>();
        DocumentViewModel vm = DocumentViewModel.Build(builder =>
        {
            builder
                .WithSize(serializableDocument.Width, serializableDocument.Height)
                .WithPalette(serializableDocument.Palette.Select(x => new PaletteColor(x.R, x.G, x.B)).ToList())
                .WithSwatches(serializableDocument.Swatches.Select(x => new PaletteColor(x.R, x.G, x.B)).ToList());

            if (serializableDocument.Groups != null)
            {
                foreach (var group in serializableDocument.Groups)
                {
                    builder.WithFolder(folderBuilder =>
                    {
                        builtLayers.AddRange(BuildFolder(
                            folderBuilder,
                            group,
                            GatherFolderLayers(group, serializableDocument.Layers),
                            serializableDocument));
                    });
                }
            }

            BuildLayers(serializableDocument.Layers.Where(x => !builtLayers.Contains(x)), builder, serializableDocument);
            SortMembersRecursively(builder.Children);
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

        folderBuilder.WithChildren(childrenBuilder =>
            {
                if (group.Subgroups != null)
                {
                    foreach (var subGroup in group.Subgroups)
                    {
                        childrenBuilder.WithFolder(subFolderBuilder =>
                        {
                            builtLayers.AddRange(BuildFolder(
                                subFolderBuilder,
                                subGroup,
                                GatherFolderLayers(subGroup, doc.Layers),
                                doc));
                        });
                    }
                }

                BuildLayers(layers, childrenBuilder, doc);
            });

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

        if (group.Subgroups is { Count: > 0 })
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
    private static void SortMembersRecursively(List<DocumentViewModelBuilder.StructureMemberBuilder> builderChildren)
    {
        builderChildren.Sort(Comparer<DocumentViewModelBuilder.StructureMemberBuilder>.Create((a, b) => a.OrderInStructure - b.OrderInStructure));
        
        foreach (var child in builderChildren)
        {
            if (child is not DocumentViewModelBuilder.FolderBuilder folderBuilder)
                continue;
            SortMembersRecursively(folderBuilder.Children);
        }
    }
}
