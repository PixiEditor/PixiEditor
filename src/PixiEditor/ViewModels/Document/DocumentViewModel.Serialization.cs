using System.Collections;
using System.Drawing;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.IO.FileEncoders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Numerics;
using PixiEditor.Parser;
using PixiEditor.Parser.Collections;
using PixiEditor.Parser.Graph;
using PixiEditor.Parser.Skia;
using PixiEditor.Parser.Skia.Encoders;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;
using PixiEditor.ViewModels.Document.Nodes;
using IKeyFrameChildrenContainer = PixiEditor.ChangeableDocument.Changeables.Interfaces.IKeyFrameChildrenContainer;
using KeyFrameData = PixiEditor.Parser.KeyFrameData;
using PixiDocument = PixiEditor.Parser.Document;
using ReferenceLayer = PixiEditor.Parser.ReferenceLayer;

namespace PixiEditor.ViewModels.Document;

internal partial class DocumentViewModel
{
    public PixiDocument ToSerializable()
    {
        NodeGraph graph = new();
        ImageEncoder encoder = new QoiEncoder();
        var serializationConfig = new SerializationConfig(encoder);
        var doc = Internals.Tracker.Document;

        Dictionary<Guid, int> nodeIdMap = new();
        Dictionary<Guid, int> keyFrameIdMap = new();

        List<SerializationFactory> factories =
            ViewModelMain.Current.Services.GetServices<SerializationFactory>().ToList();

        AddNodes(doc.NodeGraph, graph, nodeIdMap, keyFrameIdMap, serializationConfig, factories);

        var document = new PixiDocument
        {
            Width = Width,
            Height = Height,
            Swatches = ToCollection(Swatches),
            Palette = ToCollection(Palette),
            Graph = graph,
            PreviewImage =
                (TryRenderWholeImage(0).Value as Surface)?.DrawingSurface.Snapshot().Encode().AsSpan().ToArray(),
            ReferenceLayer = GetReferenceLayer(doc, serializationConfig),
            AnimationData = ToAnimationData(doc.AnimationData, doc.NodeGraph, nodeIdMap, keyFrameIdMap),
            ImageEncoderUsed = encoder.EncodedFormatName
        };

        return document;
    }

    public SvgDocument ToSvgDocument(KeyFrameTime atTime, VecI exportSize)
    {
        SvgDocument svgDocument = new(new RectD(0, 0, exportSize.X, exportSize.Y));

        float resizeFactorX = (float)exportSize.X / Width;
        float resizeFactorY = (float)exportSize.Y / Height;
        VecD resizeFactor = new VecD(resizeFactorX, resizeFactorY);

        AddElements(NodeGraph.StructureTree.Members, svgDocument, atTime, resizeFactor);

        return svgDocument;
    }

    private void AddElements(IEnumerable<INodeHandler> root, IElementContainer elementContainer, KeyFrameTime atTime,
        VecD resizeFactor)
    {
        foreach (var member in root)
        {
            if (member is FolderNodeViewModel folderNodeViewModel)
            {
                var group = new SvgGroup();

                AddElements(folderNodeViewModel.Children, group, atTime, resizeFactor);
                elementContainer.Children.Add(group);
            }

            if (member is IRasterLayerHandler)
            {
                AddSvgImage(elementContainer, atTime, member, resizeFactor);
            }
        }
    }

    private void AddSvgImage(IElementContainer elementContainer, KeyFrameTime atTime, INodeHandler member,
        VecD resizeFactor)
    {
        IReadOnlyImageNode imageNode = (IReadOnlyImageNode)Internals.Tracker.Document.FindNode(member.Id);

        var tightBounds = imageNode.GetTightBounds(atTime);

        if (tightBounds == null || tightBounds.Value.IsZeroArea) return;

        SvgImage image = new SvgImage();

        RectI bounds = (RectI)tightBounds.Value;

        using Surface surface = new Surface(bounds.Size);
        imageNode.GetLayerImageAtFrame(atTime.Frame).DrawMostUpToDateRegionOn(
            (RectI)tightBounds.Value, ChunkResolution.Full, surface.DrawingSurface, VecI.Zero);

        byte[] targetBytes;

        RectD targetBounds = tightBounds.Value;

        if (!resizeFactor.AlmostEquals(new VecD(1, 1)))
        {
            VecI newSize = new VecI((int)(bounds.Width * resizeFactor.X), (int)(bounds.Height * resizeFactor.Y));
            using var resized = surface.Resize(newSize, ResizeMethod.NearestNeighbor);
            using var snapshot = resized.DrawingSurface.Snapshot();
            targetBytes = snapshot.Encode().AsSpan().ToArray();
            
            targetBounds = new RectD(targetBounds.X * resizeFactor.X, targetBounds.Y * resizeFactor.Y, newSize.X, newSize.Y);
        }
        else
        {
            using var snapshot = surface.DrawingSurface.Snapshot();
            targetBytes = snapshot.Encode().AsSpan().ToArray();
        }

        image.X.Unit = SvgNumericUnit.FromUserUnits(targetBounds.X);
        image.Y.Unit = SvgNumericUnit.FromUserUnits(targetBounds.Y);
        image.Width.Unit = SvgNumericUnit.FromUserUnits(targetBounds.Width);
        image.Height.Unit = SvgNumericUnit.FromUserUnits(targetBounds.Height);
        image.Href.Unit = new SvgStringUnit($"data:image/png;base64,{Convert.ToBase64String(targetBytes)}");

        elementContainer.Children.Add(image);
    }

    private static void AddNodes(IReadOnlyNodeGraph graph, NodeGraph targetGraph,
        Dictionary<Guid, int> nodeIdMap,
        Dictionary<Guid, int> keyFrameIdMap,
        SerializationConfig config, IReadOnlyList<SerializationFactory> allFactories)
    {
        targetGraph.AllNodes = new List<Node>();

        int id = 0;
        int keyFrameId = 0;
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
                    Value = SerializationUtil.SerializeObject(node.InputProperties[i].NonOverridenValue, config,
                        allFactories)
                };
            }

            Dictionary<string, object> additionalData = new();
            node.SerializeAdditionalData(additionalData);

            KeyFrameData[] keyFrames = new KeyFrameData[node.KeyFrames.Count];

            for (int i = 0; i < node.KeyFrames.Count; i++)
            {
                keyFrameIdMap[node.KeyFrames[i].KeyFrameGuid] = keyFrameId + 1;
                keyFrames[i] = new KeyFrameData
                {
                    Id = keyFrameId + 1,
                    Data = SerializationUtil.SerializeObject(node.KeyFrames[i].Data, config, allFactories),
                    AffectedElement = node.KeyFrames[i].AffectedElement,
                    StartFrame = node.KeyFrames[i].StartFrame,
                    Duration = node.KeyFrames[i].Duration,
                    IsVisible = node.KeyFrames[i].IsVisible
                };

                keyFrameId++;
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

    private static ReferenceLayer? GetReferenceLayer(IReadOnlyDocument document, SerializationConfig config)
    {
        if (document.ReferenceLayer == null)
        {
            return null;
        }

        var layer = document.ReferenceLayer;

        var shape = layer.Shape;
        var imageSize = layer.ImageSize;

        var imageBytes = config.Encoder.Encode(layer.ImageBgra8888Bytes.ToArray(), imageSize.X, imageSize.Y);

        return new ReferenceLayer
        {
            Enabled = layer.IsVisible,
            Topmost = layer.IsTopMost,
            ImageWidth = imageSize.X,
            ImageHeight = imageSize.Y,
            Corners = new Corners
            {
                TopLeft = shape.TopLeft.ToVector2(),
                TopRight = shape.TopRight.ToVector2(),
                BottomLeft = shape.BottomLeft.ToVector2(),
                BottomRight = shape.BottomRight.ToVector2()
            },
            ImageBytes = imageBytes
        };
    }

    private ColorCollection ToCollection(IList<PaletteColor> collection) =>
        new(collection.Select(x => Color.FromArgb(255, x.R, x.G, x.B)));

    private AnimationData ToAnimationData(IReadOnlyAnimationData animationData, IReadOnlyNodeGraph graph,
        Dictionary<Guid, int> nodeIdMap, Dictionary<Guid, int> keyFrameIds)
    {
        var animData = new AnimationData();
        animData.KeyFrameGroups = new List<KeyFrameGroup>();
        animData.FrameRate = animationData.FrameRate;
        animData.OnionFrames = animationData.OnionFrames;
        animData.OnionOpacity = animationData.OnionOpacity;
        BuildKeyFrames(animationData.KeyFrames, animData, graph, nodeIdMap, keyFrameIds);

        return animData;
    }

    private static void BuildKeyFrames(IReadOnlyList<IReadOnlyKeyFrame> root, AnimationData animationData,
        IReadOnlyNodeGraph graph,
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
                        BuildRasterKeyFrame(rasterKeyFrame, graph, group, nodeIdMap, keyFrameIds);
                    }
                }

                animationData?.KeyFrameGroups.Add(group);
            }
        }
    }

    private static void BuildRasterKeyFrame(IReadOnlyRasterKeyFrame rasterKeyFrame, IReadOnlyNodeGraph graph,
        KeyFrameGroup group,
        Dictionary<Guid, int> idMap, Dictionary<Guid, int> keyFrameIds)
    {
        IReadOnlyChunkyImage image = rasterKeyFrame.GetTargetImage(graph.AllNodes);
        var bounds = image.FindChunkAlignedMostUpToDateBounds();

        DrawingSurface surface = null;

        if (bounds != null)
        {
            surface = DrawingBackendApi.Current.SurfaceImplementation.Create(
                new ImageInfo(bounds.Value.Width, bounds.Value.Height));

            image.DrawMostUpToDateRegionOn(
                new RectI(0, 0, bounds.Value.Width, bounds.Value.Height), ChunkResolution.Full, surface,
                new VecI(0, 0));
        }

        group.Children.Add(new ElementKeyFrame()
        {
            NodeId = idMap[rasterKeyFrame.NodeId], KeyFrameId = keyFrameIds[rasterKeyFrame.Id],
        });
    }
}
