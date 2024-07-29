using System.Collections;
using System.Drawing;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Models.IO.FileEncoders;
using PixiEditor.AvaloniaUI.Models.Serialization;
using PixiEditor.AvaloniaUI.Models.Serialization.Factories;
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
        NodeGraph graph = new();
        ImageEncoder encoder = new QoiEncoder();
        var doc = Internals.Tracker.Document;
        
        Dictionary<Guid, int> nodeIdMap = new();
        Dictionary<Guid, int> keyFrameIdMap = new();

        List<SerializationFactory> factories =
            ViewModelMain.Current.Services.GetServices<SerializationFactory>().ToList(); // a bit ugly, sorry

        AddNodes(doc.NodeGraph, graph, nodeIdMap, keyFrameIdMap, new SerializationConfig(encoder), factories);

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
            AnimationData = ToAnimationData(doc.AnimationData, nodeIdMap, keyFrameIdMap),
            ImageEncoderUsed = encoder.EncodedFormatName
        };

        return document;
    }

    private static void AddNodes(IReadOnlyNodeGraph graph, NodeGraph targetGraph, 
        Dictionary<Guid, int> nodeIdMap,
        Dictionary<Guid, int> keyFrameIdMap,
        SerializationConfig config, IReadOnlyList<SerializationFactory> allFactories)
    {
        targetGraph.AllNodes = new List<Node>();

        int id = 0;
        foreach (var node in graph.AllNodes)
        {
            nodeIdMap[node.Id] = id + 1;
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
                    Value = SerializationUtil.SerializeObject(node.InputProperties[i].NonOverridenValue, config, allFactories)
                };
            }

            Dictionary<string, object> additionalData = new();
            node.SerializeAdditionalData(additionalData);

            KeyFrameData[] keyFrames = new KeyFrameData[node.KeyFrames.Count];
            
            for (int i = 0; i < node.KeyFrames.Count; i++)
            {
                keyFrameIdMap[node.KeyFrames[i].KeyFrameGuid] = i + 1;
                keyFrames[i] = new KeyFrameData
                {
                    Id = i + 1, 
                    Data = SerializationUtil.SerializeObject(node.KeyFrames[i].Data, config, allFactories), 
                    AffectedElement = node.KeyFrames[i].AffectedElement,
                    StartFrame = node.KeyFrames[i].StartFrame, 
                    Duration = node.KeyFrames[i].Duration,
                    IsVisible = node.KeyFrames[i].IsVisible
                };
            }
                
            Dictionary<string, object> converted = ConvertToSerializable(additionalData, config, allFactories);

            List<PropertyConnection> connections = new();

            foreach (var inputProp in node.InputProperties)
            {
                if (inputProp.Connection != null)
                {
                    connections.Add(new PropertyConnection()
                    {
                        OutputNodeId = nodeIdMap[inputProp.Connection.Node.Id],
                        OutputPropertyName = inputProp.Connection.InternalPropertyName,
                        InputPropertyName = inputProp.InternalPropertyName
                    });
                }
            }

            Node parserNode = new Node()
            {
                Id = nodeIdMap[node.Id],
                Name = node.DisplayName,
                UniqueNodeName = node.GetNodeTypeUniqueName(),
                Position = node.Position.ToVector2(),
                InputPropertyValues = properties,
                AdditionalData = converted,
                KeyFrames = keyFrames,
                InputConnections = connections.ToArray()
            };

            targetGraph.AllNodes.Add(parserNode);
        }
    }

    private static Dictionary<string, object> ConvertToSerializable(
        Dictionary<string, object> additionalData,
        SerializationConfig config,
        IReadOnlyList<SerializationFactory> allFactories)
    {
        Dictionary<string, object> converted = new();
        foreach (var (key, value) in additionalData)
        {
            if (value is IEnumerable enumerable)
            {
                List<object> list = new();
                foreach (var item in enumerable)
                {
                    list.Add(SerializationUtil.SerializeObject(item, config, allFactories));
                }

                converted[key] = list;
            }
            else
            {
                converted[key] = SerializationUtil.SerializeObject(value, config, allFactories);
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

    private AnimationData ToAnimationData(IReadOnlyAnimationData animationData, Dictionary<Guid, int> nodeIdMap, Dictionary<Guid, int> keyFrameIds)
    {
        var animData = new AnimationData();
        animData.KeyFrameGroups = new List<KeyFrameGroup>();
        animData.FrameRate = animationData.FrameRate;
        BuildKeyFrames(animationData.KeyFrames, animData, nodeIdMap, keyFrameIds);

        return animData;
    }

    private static void BuildKeyFrames(IReadOnlyList<IReadOnlyKeyFrame> root, AnimationData animationData,
        Dictionary<Guid, int> nodeIdMap, Dictionary<Guid, int> keyFrameIds)
    {
        foreach (var keyFrame in root)
        {
            if (keyFrame is IKeyFrameChildrenContainer container)
            {
                KeyFrameGroup group = new();
                group.NodeId = nodeIdMap[keyFrame.NodeId];
                group.Enabled = keyFrame.IsVisible;

                foreach (var child in container.Children)
                {
                    if (child is IReadOnlyRasterKeyFrame rasterKeyFrame)
                    {
                        BuildRasterKeyFrame(rasterKeyFrame, group, nodeIdMap, keyFrameIds);
                    }
                }

                animationData?.KeyFrameGroups.Add(group);
            }
        }
    }

    private static void BuildRasterKeyFrame(IReadOnlyRasterKeyFrame rasterKeyFrame, KeyFrameGroup group,
        Dictionary<Guid, int> idMap, Dictionary<Guid, int> keyFrameIds)
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

        group.Children.Add(new ElementKeyFrame()
        {
            NodeId = idMap[rasterKeyFrame.NodeId],
            KeyFrameId = keyFrameIds[rasterKeyFrame.Id],
        });
    }
}
