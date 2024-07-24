using System.Collections;
using System.Drawing;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Models.IO.FileEncoders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Collections;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;
using PixiEditor.Parser.Skia.Encoders;
using IKeyFrameChildrenContainer = PixiEditor.ChangeableDocument.Changeables.Interfaces.IKeyFrameChildrenContainer;
using PixiDocument = PixiEditor.Parser.Document;
using ReferenceLayer = PixiEditor.Parser.ReferenceLayer;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal partial class DocumentViewModel
{
    public PixiDocument ToSerializable()
    {
        Parser.Graph.NodeGraph graph = new();
        ImageEncoder encoder = new QoiEncoder();
        var doc = Internals.Tracker.Document;

        Dictionary<Guid, int> idMap = new();

        AddNodes(doc.NodeGraph, graph, idMap, encoder);

        var document = new PixiDocument
        {
            Width = Width,
            Height = Height,
            Swatches = ToCollection(Swatches),
            Palette = ToCollection(Palette),
            Graph = graph,
            PreviewImage =
                (TryRenderWholeImage(0).Value as Surface)?.DrawingSurface.Snapshot().Encode().AsSpan().ToArray(),
            ReferenceLayer = GetReferenceLayer(doc),
            AnimationData = ToAnimationData(doc.AnimationData, idMap),
            ImageEncoderUsed = encoder.EncodedFormatName
        };

        return document;
    }

    private static void AddNodes(IReadOnlyNodeGraph graph, NodeGraph targetGraph, Dictionary<Guid, int> idMap,
        ImageEncoder encoder)
    {
        targetGraph.AllNodes = new List<Node>();

        int id = 0;
        foreach (var node in graph.AllNodes)
        {
            idMap[node.Id] = id + 1;
            id++;
        }
        
        foreach (var node in graph.AllNodes)
        {
            NodePropertyValue[] properties = new NodePropertyValue[node.InputProperties.Count];

            for (int i = 0; i < node.InputProperties.Count(); i++)
            {
                properties[i] = new NodePropertyValue()
                {
                    PropertyName = node.InputProperties[i].InternalPropertyName,
                    Value = SerializationUtil.SerializeObject(node.InputProperties[i].NonOverridenValue, encoder)
                };
            }

            Dictionary<string, object> additionalData = new();
            node.SerializeAdditionalData(additionalData);

            Dictionary<string, object> converted = ConvertToSerializable(additionalData, encoder);

            List<PropertyConnection> connections = new();

            foreach (var inputProp in node.InputProperties)
            {
                if (inputProp.Connection != null)
                {
                    connections.Add(new PropertyConnection()
                    {
                        OutputNodeId = idMap[inputProp.Connection.Node.Id],
                        OutputPropertyName = inputProp.Connection.InternalPropertyName,
                        InputPropertyName = inputProp.InternalPropertyName
                    });
                }
            }

            Node parserNode = new Node()
            {
                Id = idMap[node.Id],
                Name = node.DisplayName,
                UniqueNodeName = node.GetNodeTypeUniqueName(),
                Position = node.Position.ToVector2(),
                InputPropertyValues = properties,
                AdditionalData = converted,
                InputConnections = connections.ToArray()
            };

            targetGraph.AllNodes.Add(parserNode);
        }
    }

    private static Dictionary<string, object> ConvertToSerializable(
        Dictionary<string, object> additionalData,
        ImageEncoder encoder)
    {
        Dictionary<string, object> converted = new();
        foreach (var (key, value) in additionalData)
        {
            if (value is IEnumerable enumerable)
            {
                List<object> list = new();
                foreach (var item in enumerable)
                {
                    list.Add(SerializationUtil.SerializeObject(item, encoder));
                }

                converted[key] = list;
            }
            else
            {
                converted[key] = SerializationUtil.SerializeObject(value, encoder);
            }
        }

        return converted;
    }

    private static ReferenceLayer? GetReferenceLayer(IReadOnlyDocument document)
    {
        if (document.ReferenceLayer == null)
        {
            return null;
        }

        var layer = document.ReferenceLayer!;

        var surface = new Surface(new VecI(layer.ImageSize.X, layer.ImageSize.Y));

        surface.DrawBytes(surface.Size, layer.ImageBgra8888Bytes.ToArray(), ColorType.Bgra8888, AlphaType.Premul);

        var encoder = new UniversalFileEncoder(EncodedImageFormat.Png);

        using var stream = new MemoryStream();

        encoder.Save(stream, surface);

        stream.Position = 0;

        return new ReferenceLayer
        {
            Enabled = layer.IsVisible,
            Width = (float)layer.Shape.RectSize.X,
            Height = (float)layer.Shape.RectSize.Y,
            OffsetX = (float)layer.Shape.TopLeft.X,
            OffsetY = (float)layer.Shape.TopLeft.Y,
            Corners = new Corners
            {
                TopLeft = layer.Shape.TopLeft.ToVector2(),
                TopRight = layer.Shape.TopRight.ToVector2(),
                BottomLeft = layer.Shape.BottomLeft.ToVector2(),
                BottomRight = layer.Shape.BottomRight.ToVector2()
            },
            Opacity = 1,
            ImageBytes = stream.ToArray()
        };
    }

    private ColorCollection ToCollection(IList<PaletteColor> collection) =>
        new(collection.Select(x => Color.FromArgb(255, x.R, x.G, x.B)));

    private AnimationData ToAnimationData(IReadOnlyAnimationData animationData, Dictionary<Guid, int> idMap)
    {
        var animData = new AnimationData();
        animData.KeyFrameGroups = new List<KeyFrameGroup>();
        BuildKeyFrames(animationData.KeyFrames, animData, idMap);

        return animData;
    }

    private static void BuildKeyFrames(IReadOnlyList<IReadOnlyKeyFrame> root, AnimationData animationData,
        Dictionary<Guid, int> idMap)
    {
        foreach (var keyFrame in root)
        {
            if (keyFrame is IKeyFrameChildrenContainer container)
            {
                KeyFrameGroup group = new();
                group.NodeId = idMap[keyFrame.NodeId];
                group.Enabled = keyFrame.IsVisible;

                foreach (var child in container.Children)
                {
                    if (child is IKeyFrameChildrenContainer groupKeyFrame)
                    {
                        BuildKeyFrames(groupKeyFrame.Children, null, idMap);
                    }
                    else if (child is IReadOnlyRasterKeyFrame rasterKeyFrame)
                    {
                        BuildRasterKeyFrame(rasterKeyFrame, group, idMap);
                    }
                }

                animationData?.KeyFrameGroups.Add(group);
            }
        }
    }

    private static void BuildRasterKeyFrame(IReadOnlyRasterKeyFrame rasterKeyFrame, KeyFrameGroup group,
        Dictionary<Guid, int> idMap)
    {
        var bounds = rasterKeyFrame.Image.FindChunkAlignedMostUpToDateBounds();

        DrawingSurface surface = null;

        if (bounds != null)
        {
            surface = DrawingBackendApi.Current.SurfaceImplementation.Create(
                new ImageInfo(bounds.Value.Width, bounds.Value.Height));

            rasterKeyFrame.Image.DrawMostUpToDateRegionOn(
                new RectI(0, 0, bounds.Value.Width, bounds.Value.Height), ChunkResolution.Full, surface,
                new VecI(0, 0));
        }

        group.Children.Add(new RasterKeyFrame()
        {
            NodeId = idMap[rasterKeyFrame.NodeId],
            StartFrame = rasterKeyFrame.StartFrame,
            Duration = rasterKeyFrame.Duration,
        });
    }
}
