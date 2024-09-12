using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.IO.FileEncoders;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
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
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Features;
using PixiEditor.SVG.Units;
using PixiEditor.ViewModels.Document.Nodes;
using BlendMode = PixiEditor.DrawingApi.Core.Surfaces.BlendMode;
using Color = System.Drawing.Color;
using IKeyFrameChildrenContainer = PixiEditor.ChangeableDocument.Changeables.Interfaces.IKeyFrameChildrenContainer;
using KeyFrameData = PixiEditor.Parser.KeyFrameData;
using Node = PixiEditor.Parser.Graph.Node;
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

    public SvgDocument ToSvgDocument(KeyFrameTime atTime, VecI exportSize, VectorExportConfig? vectorExportConfig)
    {
        SvgDocument svgDocument = new(new RectD(0, 0, exportSize.X, exportSize.Y));

        float resizeFactorX = (float)exportSize.X / Width;
        float resizeFactorY = (float)exportSize.Y / Height;
        VecD resizeFactor = new VecD(resizeFactorX, resizeFactorY);

        AddElements(NodeGraph.StructureTree.Members, svgDocument, atTime, resizeFactor, vectorExportConfig);

        return svgDocument;
    }

    private void AddElements(IEnumerable<INodeHandler> root, IElementContainer elementContainer, KeyFrameTime atTime,
        VecD resizeFactor, VectorExportConfig? vectorExportConfig)
    {
        foreach (var member in root)
        {
            if (member is FolderNodeViewModel folderNodeViewModel)
            {
                var group = new SvgGroup();

                AddElements(folderNodeViewModel.Children, group, atTime, resizeFactor, vectorExportConfig);
                elementContainer.Children.Add(group);
            }

            if (member is IRasterLayerHandler)
            {
                AddSvgImage(elementContainer, atTime, member, resizeFactor,
                    vectorExportConfig?.UseNearestNeighborForImageUpscaling ?? false);
            }
            else if (member is IVectorLayerHandler vectorLayerHandler)
            {
                AddSvgShape(elementContainer, vectorLayerHandler, resizeFactor);
            }
        }
    }

    private void AddSvgShape(IElementContainer elementContainer, IVectorLayerHandler vectorLayerHandler,
        VecD resizeFactor)
    {
        IReadOnlyVectorNode vectorNode =
            (IReadOnlyVectorNode)Internals.Tracker.Document.FindNode(vectorLayerHandler.Id);

        SvgElement? elementToAdd = null;

        if (vectorNode.ShapeData is IReadOnlyEllipseData ellipseData)
        {
            elementToAdd = AddEllipse(resizeFactor, ellipseData);
        }
        else if (vectorNode.ShapeData is IReadOnlyRectangleData rectangleData)
        {
            elementToAdd = AddRectangle(resizeFactor, rectangleData);
        }
        else if (vectorNode.ShapeData is IReadOnlyLineData lineData)
        {
            elementToAdd = AddLine(resizeFactor, lineData);
        }

        if (elementToAdd != null)
        {
            elementContainer.Children.Add(elementToAdd);
        }
    }

    private static SvgLine AddLine(VecD resizeFactor, IReadOnlyLineData lineData)
    {
        SvgLine line = new SvgLine();
        line.X1.Unit = SvgNumericUnit.FromUserUnits(lineData.Start.X * resizeFactor.X);
        line.Y1.Unit = SvgNumericUnit.FromUserUnits(lineData.Start.Y * resizeFactor.Y);
        line.X2.Unit = SvgNumericUnit.FromUserUnits(lineData.End.X * resizeFactor.X);
        line.Y2.Unit = SvgNumericUnit.FromUserUnits(lineData.End.Y * resizeFactor.Y);
        
        line.Stroke.Unit = SvgColorUnit.FromRgba(lineData.StrokeColor.R, lineData.StrokeColor.G,
            lineData.StrokeColor.B, lineData.StrokeColor.A);
        line.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(lineData.StrokeWidth * resizeFactor.X);
        line.Transform.Unit = new SvgTransformUnit(lineData.TransformationMatrix);
        
        return line;
    }

    private static SvgEllipse AddEllipse(VecD resizeFactor, IReadOnlyEllipseData ellipseData)
    {
        SvgEllipse ellipse = new SvgEllipse();
        ellipse.Cx.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Center.X * resizeFactor.X);
        ellipse.Cy.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Center.Y * resizeFactor.Y);
        ellipse.Rx.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Radius.X * resizeFactor.X);
        ellipse.Ry.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Radius.Y * resizeFactor.Y);
        ellipse.Fill.Unit = SvgColorUnit.FromRgba(ellipseData.FillColor.R, ellipseData.FillColor.G,
            ellipseData.FillColor.B, ellipseData.FillColor.A);
        ellipse.Stroke.Unit = SvgColorUnit.FromRgba(ellipseData.StrokeColor.R, ellipseData.StrokeColor.G,
            ellipseData.StrokeColor.B, ellipseData.StrokeColor.A);
        ellipse.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(ellipseData.StrokeWidth);
        ellipse.Transform.Unit = new SvgTransformUnit(ellipseData.TransformationMatrix);

        return ellipse;
    }

    private SvgRectangle AddRectangle(VecD resizeFactor, IReadOnlyRectangleData rectangleData)
    {
        SvgRectangle rect = new SvgRectangle();
        rect.X.Unit =
            SvgNumericUnit.FromUserUnits(rectangleData.Center.X * resizeFactor.X -
                                         rectangleData.Size.X / 2 * resizeFactor.X);
        rect.Y.Unit =
            SvgNumericUnit.FromUserUnits(rectangleData.Center.Y * resizeFactor.Y -
                                         rectangleData.Size.Y / 2 * resizeFactor.Y);
        rect.Width.Unit = SvgNumericUnit.FromUserUnits(rectangleData.Size.X * resizeFactor.X);
        rect.Height.Unit = SvgNumericUnit.FromUserUnits(rectangleData.Size.Y * resizeFactor.Y);
        rect.Fill.Unit = SvgColorUnit.FromRgba(rectangleData.FillColor.R, rectangleData.FillColor.G,
            rectangleData.FillColor.B, rectangleData.FillColor.A);
        rect.Stroke.Unit = SvgColorUnit.FromRgba(rectangleData.StrokeColor.R, rectangleData.StrokeColor.G,
            rectangleData.StrokeColor.B, rectangleData.StrokeColor.A);
        rect.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(rectangleData.StrokeWidth);
        rect.Transform.Unit = new SvgTransformUnit(rectangleData.TransformationMatrix);
        
        return rect;
    }

    private void AddSvgImage(IElementContainer elementContainer, KeyFrameTime atTime, INodeHandler member,
        VecD resizeFactor, bool useNearestNeighborForImageUpscaling)
    {
        IReadOnlyImageNode imageNode = (IReadOnlyImageNode)Internals.Tracker.Document.FindNode(member.Id);

        var tightBounds = imageNode.GetTightBounds(atTime);

        if (tightBounds == null || tightBounds.Value.IsZeroArea) return;

        Image toSave = null;
        DrawingBackendApi.Current.RenderingServer.Invoke(() =>
        {
            using Texture rendered = Renderer.RenderLayer(imageNode.Id, ChunkResolution.Full, atTime.Frame);

            using Surface surface = new Surface(rendered.Size);
            surface.DrawingSurface.Canvas.DrawImage(rendered.DrawingSurface.Snapshot(), 0, 0);

            toSave = surface.DrawingSurface.Snapshot((RectI)tightBounds.Value);
        });

        var image = CreateImageElement(resizeFactor, tightBounds.Value, toSave, useNearestNeighborForImageUpscaling);

        elementContainer.Children.Add(image);
    }

    private static SvgImage CreateImageElement(VecD resizeFactor, RectD tightBounds,
        Image toSerialize, bool useNearestNeighborForImageUpscaling)
    {
        SvgImage image = new SvgImage();

        RectI bounds = (RectI)tightBounds;

        using Surface surface = new Surface(bounds.Size);
        surface.DrawingSurface.Canvas.DrawImage(toSerialize, 0, 0);

        byte[] targetBytes;

        RectD targetBounds = tightBounds;

        if (!resizeFactor.AlmostEquals(new VecD(1, 1)))
        {
            VecI newSize = new VecI((int)(bounds.Width * resizeFactor.X), (int)(bounds.Height * resizeFactor.Y));
            using var resized = surface.Resize(newSize, ResizeMethod.NearestNeighbor);
            using var snapshot = resized.DrawingSurface.Snapshot();
            targetBytes = snapshot.Encode().AsSpan().ToArray();

            targetBounds = new RectD(targetBounds.X * resizeFactor.X, targetBounds.Y * resizeFactor.Y, newSize.X,
                newSize.Y);
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

        if (useNearestNeighborForImageUpscaling)
        {
            image.ImageRendering.Unit = new SvgEnumUnit<SvgImageRenderingType>(SvgImageRenderingType.Pixelated);
        }

        return image;
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
