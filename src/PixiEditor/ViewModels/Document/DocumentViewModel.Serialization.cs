using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
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
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.IO;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
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
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;
using Color = System.Drawing.Color;
using IKeyFrameChildrenContainer = PixiEditor.ChangeableDocument.Changeables.Interfaces.IKeyFrameChildrenContainer;
using KeyFrameData = PixiEditor.Parser.KeyFrameData;
using Node = PixiEditor.Parser.Graph.Node;
using NodeGraph = PixiEditor.Parser.Graph.NodeGraph;
using PixiDocument = PixiEditor.Parser.Document;
using ReferenceLayer = PixiEditor.Parser.ReferenceLayer;
using Variable = PixiEditor.Parser.Graph.Variable;

namespace PixiEditor.ViewModels.Document;

internal partial class DocumentViewModel
{
    public PixiDocument ToSerializable()
    {
        NodeGraph graph = new();
        ImageEncoder encoder = new QoiEncoder();
        var serializationConfig = new SerializationConfig(encoder, ProcessingColorSpace);
        var doc = Internals.Tracker.Document;

        Dictionary<Guid, int> nodeIdMap = new();
        Dictionary<Guid, int> keyFrameIdMap = new();

        ResourceStorage storage = new ResourceStorage();

        List<SerializationFactory> factories =
            ViewModelMain.Current.Services.GetServices<SerializationFactory>().ToList();

        foreach (var factory in factories)
        {
            factory.Storage = storage;
        }

        AddBlackboard(doc.Blackboard, graph, serializationConfig, factories);
        AddNodes(doc, graph, nodeIdMap, keyFrameIdMap, serializationConfig, factories);

        var preview = TryRenderWholeImage(0);
        byte[]? previewBytes = null;
        if (preview.IsT1)
        {
            previewBytes = preview.AsT1.DrawingSurface.Snapshot().Encode().AsSpan().ToArray();
            preview.AsT1.Dispose();
        }

        var document = new PixiDocument
        {
            SerializerName = "PixiEditor",
            SerializerVersion = VersionHelpers.GetCurrentAssemblyVersion().ToString(),
            SrgbColorBlending = doc.ProcessingColorSpace.IsSrgb,
            Width = Width,
            Height = Height,
            Swatches = ToCollection(Swatches),
            Palette = ToCollection(Palette),
            Graph = graph,
            PreviewImage = previewBytes,
            ReferenceLayer = GetReferenceLayer(doc, serializationConfig),
            AnimationData = ToAnimationData(doc.AnimationData, doc.NodeGraph, nodeIdMap, keyFrameIdMap),
            ImageEncoderUsed = encoder.EncodedFormatName,
            Resources = storage
        };

        return document;
    }

    public SvgDocument ToSvgDocument(KeyFrameTime atTime, VecI exportSize, VectorExportConfig? vectorExportConfig)
    {
        return ToSvgDocument(Internals.Tracker.Document, atTime, exportSize, vectorExportConfig);
    }

    public static SvgDocument ToSvgDocument(IReadOnlyDocument document, KeyFrameTime atTime, VecI exportSize,
        VectorExportConfig? vectorExportConfig)
    {
        SvgDocument svgDocument = new(new RectD(0, 0, exportSize.X, exportSize.Y));

        float resizeFactorX = (float)exportSize.X / document.Size.X;
        float resizeFactorY = (float)exportSize.Y / document.Size.Y;
        VecD resizeFactor = new VecD(resizeFactorX, resizeFactorY);

        var root = document.GetStructureTreeInOrder().Where(x => x.IsVisible.Value).Reverse().ToList();

        AddElements(document, root, svgDocument, atTime, exportSize, resizeFactor, vectorExportConfig,
            svgDocument.Defs);

        return svgDocument;
    }

    private static void AddElements(IReadOnlyDocument doc, IEnumerable<IReadOnlyStructureNode> root,
        IElementContainer elementContainer,
        KeyFrameTime atTime, VecI exportSize,
        VecD resizeFactor, VectorExportConfig? vectorExportConfig, SvgDefs defs)
    {
        foreach (var member in root)
        {
            if (member is FolderNode folderNode)
            {
                var group = new SvgGroup
                {
                    Opacity = { Unit = new SvgNumericUnit(folderNode.Opacity.Value, "") },
                    Id = { Unit = new SvgStringUnit(folderNode.DisplayName) },
                };

                if (folderNode.ClipToPreviousMember &&
                    elementContainer.Children.Count > 0)
                {
                    IReadOnlyStructureNode? previousMember =
                        folderNode.Background.Connection
                            ?.Node as IReadOnlyStructureNode;
                    var previousElement = elementContainer.Children.LastOrDefault();

                    AddToClipDefs(defs, previousElement, previousMember);

                    if (previousMember != null)
                    {
                        group.ClipPath.Unit = new SvgStringUnit($"url(#{previousMember.Id}_clip)");
                    }
                }

                AddElements(doc, folderNode.GetChildrenNodes().Where(x => x.IsVisible.Value).Reverse().ToList(), group,
                    atTime, exportSize, resizeFactor, vectorExportConfig, defs);
                elementContainer.Children.Add(group);
            }

            if (member is IReadOnlyImageNode img)
            {
                AddSvgImage(elementContainer, atTime, img, resizeFactor,
                    vectorExportConfig?.UseNearestNeighborForImageUpscaling ?? false, doc);
            }
            else if (member is IReadOnlyVectorNode vectorLayerHandler)
            {
                AddSvgShape(elementContainer, vectorLayerHandler, resizeFactor, defs);
            }
            else if (member is NestedDocumentNode nested)
            {
                var nestedDocument = nested.NestedDocument.Value?.DocumentInstance;
                if (nestedDocument == null)
                    continue;

                var svgDoc = ToSvgDocument(nestedDocument, atTime, exportSize, vectorExportConfig);
                svgDoc.Transform.Unit = new SvgTransformUnit(nested.TransformationMatrix);
                elementContainer.Children.Add(svgDoc);
            }
        }
    }

    private static void AddToClipDefs(SvgDefs defs, SvgElement? previousElement,
        IReadOnlyStructureNode? previousMember)
    {
        if (previousElement != null)
        {
            var clone = previousElement.Clone();
            if (clone is not IClipable clipable)
                return;
            clipable.ClipPath.Unit = null;
            clone.Id.Unit = null;
            defs.Children.Add(new SvgClipPath()
            {
                Id = { Unit = new SvgStringUnit($"{previousMember.Id}_clip") }, Children = { previousElement }
            });
        }
    }

    private static void AddSvgShape(IElementContainer elementContainer, IReadOnlyVectorNode vectorNode,
        VecD resizeFactor, SvgDefs defs)
    {
        SvgElement? elementToAdd = null;

        if (vectorNode.ShapeData is IReadOnlyEllipseData ellipseData)
        {
            elementToAdd = AddEllipse(ellipseData);
        }
        else if (vectorNode.ShapeData is IReadOnlyRectangleData rectangleData)
        {
            elementToAdd = AddRectangle(rectangleData);
        }
        else if (vectorNode.ShapeData is IReadOnlyLineData lineData)
        {
            elementToAdd = AddLine(lineData);
        }
        else if (vectorNode.ShapeData is IReadOnlyPathData shapeData)
        {
            elementToAdd = AddVectorPath(shapeData);
        }
        else if (vectorNode.ShapeData is IReadOnlyTextData textData)
        {
            elementToAdd = AddText(textData);
        }

        IReadOnlyShapeVectorData data = vectorNode.ShapeData;

        if (data != null && elementToAdd is SvgPrimitive primitive)
        {
            Matrix3X3 transform = data.TransformationMatrix;

            transform = transform.PostConcat(Matrix3X3.CreateScale((float)resizeFactor.X, (float)resizeFactor.Y));
            primitive.Transform.Unit = new SvgTransformUnit?(new SvgTransformUnit(transform));

            primitive.Id.Unit = new SvgStringUnit(vectorNode.DisplayName);

            primitive.Opacity.Unit = new SvgNumericUnit(vectorNode.Opacity.Value, "");

            Paintable finalFill = data.Fill ? data.FillPaintable : new ColorPaintable(Colors.Transparent);
            primitive.Fill.Unit = new SvgPaintServerUnit(finalFill);

            if (finalFill is ColorPaintable colorPaintable)
            {
                primitive.FillOpacity.Unit = new SvgNumericUnit(colorPaintable.Color.A / 255f, "");
            }

            primitive.Stroke.Unit = new SvgPaintServerUnit(data.Stroke);

            primitive.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(data.StrokeWidth);

            bool clipToMemberBelowEnabled = vectorNode.ClipToPreviousMember;
            if (clipToMemberBelowEnabled && elementContainer.Children.Count > 0)
            {
                IReadOnlyStructureNode? previousMember =
                    (vectorNode as VectorLayerNode).Background.Connection?.Node
                    as IReadOnlyStructureNode;

                var previousElement = elementContainer.Children[^1];

                AddToClipDefs(defs, previousElement, previousMember);

                if (previousMember != null)
                {
                    primitive.ClipPath.Unit = new SvgStringUnit($"url(#{previousMember.Id}_clip)");
                }
            }
        }
        else if (elementToAdd is SvgGroup group)
        {
            Matrix3X3 transform = data.TransformationMatrix;

            transform = transform.PostConcat(Matrix3X3.CreateScale((float)resizeFactor.X, (float)resizeFactor.Y));
            group.Transform.Unit = new SvgTransformUnit?(new SvgTransformUnit(transform));
        }

        if (elementToAdd != null)
        {
            elementContainer.Children.Add(elementToAdd);
        }
    }

    private static SvgLine AddLine(IReadOnlyLineData lineData)
    {
        SvgLine line = new SvgLine();
        line.X1.Unit = SvgNumericUnit.FromUserUnits(lineData.Start.X);
        line.Y1.Unit = SvgNumericUnit.FromUserUnits(lineData.Start.Y);
        line.X2.Unit = SvgNumericUnit.FromUserUnits(lineData.End.X);
        line.Y2.Unit = SvgNumericUnit.FromUserUnits(lineData.End.Y);

        line.Stroke.Unit = new SvgPaintServerUnit(lineData.Stroke);

        line.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(lineData.StrokeWidth);

        return line;
    }

    private static SvgEllipse AddEllipse(IReadOnlyEllipseData ellipseData)
    {
        SvgEllipse ellipse = new SvgEllipse();
        ellipse.Cx.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Center.X);
        ellipse.Cy.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Center.Y);
        ellipse.Rx.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Radius.X);
        ellipse.Ry.Unit = SvgNumericUnit.FromUserUnits(ellipseData.Radius.Y);

        return ellipse;
    }

    private static SvgRectangle AddRectangle(IReadOnlyRectangleData rectangleData)
    {
        SvgRectangle rect = new SvgRectangle();

        float centerX = (float)rectangleData.Center.X;
        float centerY = (float)rectangleData.Center.Y;
        float halfWidth = (float)rectangleData.Size.X / 2f;
        float halfHeight = (float)rectangleData.Size.Y / 2f;
        float minHalf = Math.Min(halfWidth, halfHeight);
        float clampedCorner = Math.Clamp((float)rectangleData.CornerRadius, 0f, 1f);
        float radius = minHalf * clampedCorner;
        float radiusX = Math.Min(radius, halfWidth);
        float radiusY = Math.Min(radius, halfHeight);


        rect.X.Unit = SvgNumericUnit.FromUserUnits(centerX - halfWidth);
        rect.Y.Unit = SvgNumericUnit.FromUserUnits(centerY - halfHeight);

        rect.Width.Unit = SvgNumericUnit.FromUserUnits(rectangleData.Size.X);
        rect.Height.Unit = SvgNumericUnit.FromUserUnits(rectangleData.Size.Y);

        rect.Rx.Unit = SvgNumericUnit.FromUserUnits(radiusX);
        rect.Ry.Unit = SvgNumericUnit.FromUserUnits(radiusY);

        return rect;
    }

    private static SvgPath AddVectorPath(IReadOnlyPathData data)
    {
        var path = new SvgPath();
        if (data.Path != null)
        {
            // This is super strange, we got reports that on linux with different locales, numbers are separated by commas
            // and not dots. Comma separated svg data is invalid. This is raw Skia call, there is no other place where it could be changed.
            // That's why we replace commas with dots here. I really hope skia never uses commas as a number separator
            string pathData = data.Path.ToSvgPathData().Replace(",", ".");
            path.PathData.Unit = new SvgStringUnit(pathData);
            SvgFillRule fillRule = data.Path.FillType switch
            {
                PathFillType.EvenOdd => SvgFillRule.EvenOdd,
                PathFillType.Winding => SvgFillRule.NonZero,
                PathFillType.InverseWinding => SvgFillRule.NonZero,
                PathFillType.InverseEvenOdd => SvgFillRule.EvenOdd,
            };

            path.FillRule.Unit = new SvgEnumUnit<SvgFillRule>(fillRule);
            path.StrokeLineJoin.Unit = new SvgEnumUnit<SvgStrokeLineJoin>(ToSvgLineJoin(data.StrokeLineJoin));
            path.StrokeLineCap.Unit = new SvgEnumUnit<SvgStrokeLineCap>((SvgStrokeLineCap)data.StrokeLineCap);
        }

        return path;
    }

    private static void AddSvgImage(IElementContainer elementContainer, KeyFrameTime atTime,
        IReadOnlyImageNode imageNode,
        VecD resizeFactor, bool useNearestNeighborForImageUpscaling, IReadOnlyDocument doc)
    {
        var tightBounds = imageNode.GetTightBounds(atTime);

        if (tightBounds == null || tightBounds.Value.IsZeroArea) return;

        Image toSave = null;
        DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
        {
            using Surface surface = new Surface(doc.Size);
            doc.Renderer.RenderLayer(surface.DrawingSurface, imageNode.Id, ChunkResolution.Full, atTime.Frame,
                doc.Size);

            toSave = surface.DrawingSurface.Snapshot((RectI)tightBounds.Value);
        });

        var image = CreateImageElement(resizeFactor, tightBounds.Value, toSave, useNearestNeighborForImageUpscaling);
        image.Id.Unit = new SvgStringUnit(imageNode.DisplayName);

        elementContainer.Children.Add(image);
    }

    private static SvgElement AddText(IReadOnlyTextData textData)
    {
        RichText rt = new RichText(textData.Text);
        rt.Spacing = textData.Spacing;
        rt.MaxWidth = textData.MaxWidth;

        using Font font = textData.ConstructFont();

        if (rt.Lines.Length <= 1)
        {
            return BuildTextElement(textData, textData.Text, font);
        }

        SvgGroup group = new SvgGroup();
        for (int i = 0; i < rt.Lines.Length; i++)
        {
            var offset = rt.GetLineOffset(i, font);

            var text = BuildTextElement(textData, rt.Lines[i], font);
            text.Y.Unit = SvgNumericUnit.FromUserUnits(textData.Position.Y + offset.Y);

            group.Children.Add(text);
        }

        return group;
    }

    private static SvgText BuildTextElement(IReadOnlyTextData textData, string value, Font font)
    {
        SvgText text = new SvgText();
        text.Text.Unit = new SvgStringUnit(value);
        text.X.Unit = SvgNumericUnit.FromUserUnits(textData.Position.X);
        text.Y.Unit = SvgNumericUnit.FromUserUnits(textData.Position.Y);
        text.FontSize.Unit = SvgNumericUnit.FromUserUnits(font.Size);
        text.FontFamily.Unit = new SvgStringUnit(font.Family.Name);
        text.FontWeight.Unit = new SvgEnumUnit<SvgFontWeight>(font.Bold ? SvgFontWeight.Bold : SvgFontWeight.Normal);
        text.FontStyle.Unit = new SvgEnumUnit<SvgFontStyle>(font.Italic ? SvgFontStyle.Italic : SvgFontStyle.Normal);
        text.Stroke.Unit = new SvgPaintServerUnit(textData.Stroke);
        text.StrokeWidth.Unit = SvgNumericUnit.FromUserUnits(textData.StrokeWidth);
        text.Fill.Unit =
            new SvgPaintServerUnit(textData.Fill ? textData.FillPaintable : new ColorPaintable(Colors.Transparent));

        return text;
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

    private static void AddNodes(IReadOnlyDocument doc, NodeGraph targetGraph,
        Dictionary<Guid, int> nodeIdMap,
        Dictionary<Guid, int> keyFrameIdMap,
        SerializationConfig config, IReadOnlyList<SerializationFactory> allFactories)
    {
        targetGraph.AllNodes = new List<Node>();

        int id = 0;
        int keyFrameId = 0;
        foreach (var node in doc.NodeGraph.AllNodes)
        {
            nodeIdMap[node.Id] = id + 1;
            id++;
        }

        foreach (var node in doc.NodeGraph.AllNodes)
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
            node.SerializeAdditionalData(doc, additionalData);

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

            int? pairNodeId = null;
            if (node is IPairNode pairNode)
            {
                if (pairNode.OtherNode != Guid.Empty &&
                    nodeIdMap.TryGetValue(pairNode.OtherNode, out var value))
                {
                    pairNodeId = value;
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
                InputConnections = connections.ToArray(),
                PairId = pairNodeId,
            };

            targetGraph.AllNodes.Add(parserNode);
        }
    }

    private static void AddBlackboard(IReadOnlyBlackboard blackboard, NodeGraph targetGraph,
        SerializationConfig config, IReadOnlyList<SerializationFactory> allFactories)
    {
        List<Variable> variables = new();

        foreach (var prop in blackboard.Variables)
        {
            variables.Add(new Variable()
            {
                Name = prop.Value.Name,
                Value = SerializationUtil.SerializeObject(prop.Value.Value, config, allFactories),
                Unit = prop.Value.Unit,
                Min = prop.Value.Min,
                Max = prop.Value.Max,
                IsExposed = prop.Value.IsExposed,
                Type = SerializationUtil.GetWellKnownSerializationTypeName(prop.Value.Type, allFactories)
            });
        }

        targetGraph.Blackboard = new Parser.Graph.Blackboard() { Variables = variables };
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

        var imageBytes = config.Encoder.Encode(layer.ImageBgra8888Bytes.ToArray(), imageSize.X, imageSize.Y, true);

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
        animData.DefaultEndFrame = animationData.DefaultEndFrame;
        animData.FallbackAnimationToLayerImage = animationData.FallbackAnimationToLayerImage;
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
                if (!nodeIdMap.ContainsKey(keyFrame.NodeId)) continue;

                KeyFrameGroup group = new();
                group.NodeId = nodeIdMap[keyFrame.NodeId];
                group.Enabled = keyFrame.IsVisible;

                foreach (var child in container.Children)
                {
                    if (child is IReadOnlyRasterKeyFrame rasterKeyFrame)
                    {
                        if (!nodeIdMap.ContainsKey(rasterKeyFrame.NodeId)) continue;
                        if (!keyFrameIds.ContainsKey(rasterKeyFrame.Id)) continue;

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

        if (bounds is { Width: > 0, Height: > 0 })
        {
            surface = DrawingBackendApi.Current.SurfaceImplementation.Create(
                new ImageInfo(bounds.Value.Width, bounds.Value.Height));

            image.DrawMostUpToDateRegionOn(
                new RectI(0, 0, bounds.Value.Width, bounds.Value.Height), ChunkResolution.Full, surface.Canvas,
                new VecI(0, 0));
        }

        group.Children.Add(new ElementKeyFrame()
        {
            NodeId = idMap[rasterKeyFrame.NodeId], KeyFrameId = keyFrameIds[rasterKeyFrame.Id],
        });
    }

    private static SvgStrokeLineJoin ToSvgLineJoin(StrokeJoin strokeLineJoin)
    {
        return strokeLineJoin switch
        {
            StrokeJoin.Bevel => SvgStrokeLineJoin.Bevel,
            StrokeJoin.Round => SvgStrokeLineJoin.Round,
            _ => SvgStrokeLineJoin.Miter
        };
    }
}
