using System.Diagnostics.CodeAnalysis;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Parser;
using PixiEditor.Parser.Graph;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Elements.Filters;
using PixiEditor.SVG.Enums;
using PixiEditor.SVG.Exceptions;
using PixiEditor.SVG.Units;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Models.IO.CustomDocumentFormats;

internal class SvgDocumentBuilder : IDocumentBuilder
{
    public IReadOnlyCollection<string> Extensions { get; } = [".svg"];

    public void Build(DocumentViewModelBuilder builder, string path)
    {
        string xml = File.ReadAllText(path);
        SvgDocument document = SvgDocument.Parse(xml);

        if (document == null)
        {
            throw new SvgParsingException("Failed to parse SVG document");
        }

        Build(builder, document);
    }

    private void Build(DocumentViewModelBuilder builder, SvgDocument document)
    {
        bool unknownSize = document.Width.Unit == null && document.Height.Unit == null && document.ViewBox.Unit == null;
        document.Width.Unit ??= new SvgNumericUnit(document.ViewBox.Unit?.Value.Width ?? 1024, "px");
        document.Height.Unit ??= new SvgNumericUnit(document.ViewBox.Unit?.Value.Height ?? 1024, "px");

        if (document.ViewBox.Unit == null)
        {
            document.ViewBox.Unit = new SvgRectUnit(new RectD(0, 0, document.Width.Unit.Value.PixelsValue ?? 1024,
                document.Height.Unit.Value.PixelsValue ?? 1024));
        }

        StyleContext styleContext = new(document);

        VecI size = new((int)document.Width.Unit.Value.Value, (int)document.Height.Unit.Value.Value);
        if (size.ShortestAxis < 1)
        {
            size = new VecI(1024, 1024);
        }

        builder.WithSize(size)
            .WithSrgbColorBlending(true) // apparently svgs blend colors in SRGB space
            .WithGraph(graph =>
            {
                int? lastId = null;
                foreach (SvgElement element in document.Children)
                {
                    StyleContext style = styleContext.WithElement(element);
                    if (element is SvgPrimitive primitive)
                    {
                        lastId = AddPrimitive(element, style, graph, lastId);
                    }
                    else if (element is SvgGroup group)
                    {
                        lastId = AddGroup(group, graph, style, lastId);
                    }
                    else if (element is SvgImage svgImage)
                    {
                        lastId = AddImage(svgImage, style, graph, lastId);
                    }
                    else if (element is SvgDocument nestedDocument)
                    {
                        lastId = AddNestedDocument(nestedDocument, graph, lastId, style);
                    }
                }

                graph.WithOutputNode(lastId, "Output");

                VecD pos = VecD.Zero;
                foreach (var node in graph.AllNodes)
                {
                    node.Position = new Vector2() { X = (float)pos.X, Y = (float)pos.Y };
                    pos += new VecD(250, 0);
                }
            });

        if (unknownSize)
        {
            builder.WithFitToContent();
        }
    }

    [return: NotNull]
    private int? AddPrimitive(SvgElement element, StyleContext styleContext,
        NodeGraphBuilder graph,
        int? lastId, string connectionName = "Background")
    {
        LocalizedString name = "";
        ShapeVectorData shapeData = null;
        if (element is SvgEllipse or SvgCircle)
        {
            shapeData = AddEllipse(element, styleContext);
            name = VectorEllipseToolViewModel.NewLayerKey;
        }
        else if (element is SvgLine line)
        {
            shapeData = AddLine(line, styleContext);
            name = VectorLineToolViewModel.NewLayerKey;
        }
        else if (element is SvgPath pathElement)
        {
            shapeData = AddPath(pathElement, styleContext);
            name = VectorPathToolViewModel.NewLayerKey;
        }
        else if (element is SvgRectangle rect)
        {
            shapeData = AddRect(rect, styleContext);
            name = VectorRectangleToolViewModel.NewLayerKey;
        }
        else if (element is SvgText text)
        {
            shapeData = AddText(text, styleContext);
            name = TextToolViewModel.NewLayerKey;
        }
        else if (element is SvgPolyline or SvgPolygon)
        {
            shapeData = AddPoly(element);
            name = VectorPathToolViewModel.NewLayerKey;
        }

        name = element.Id.Unit?.Value ?? name;

        AddCommonShapeData(shapeData, styleContext);
        var isVisible = element.Visibility.Unit?.Value != SvgVisibility.Hidden &&
                        element.Visibility.Unit?.Value != SvgVisibility.Collapse;

        bool hasFilter = styleContext.Filter.Unit?.ImageFilter != null;

        NodeGraphBuilder.NodeBuilder nBuilder = graph.WithNodeOfType<VectorLayerNode>(out int id)
            .WithName(name)
            .WithInputValues(new Dictionary<string, object>()
            {
                { StructureNode.OpacityPropertyName, (float)(styleContext.Opacity.Unit?.Value ?? 1f) },
                { StructureNode.IsVisiblePropertyName, isVisible }
            })
            .WithAdditionalData(new Dictionary<string, object>() { { "ShapeData", shapeData } });

        int? filterNodeId = null;
        if (hasFilter)
        {
            var imageFilter = styleContext.Filter.Unit?.ImageFilter;
            Type filterToNodeType = MapImageFilterToNodeType(imageFilter, out var props);
            graph.WithNodeOfType(filterToNodeType, out int filterId)
                .WithId(filterId)
                .WithInputValues(props);


            filterNodeId = filterId;
        }

        List<PropertyConnection> connections = new();
        if (lastId != null)
        {
            connections.Add(new PropertyConnection()
            {
                InputPropertyName = connectionName, OutputPropertyName = "Output", OutputNodeId = lastId.Value
            });
        }

        if (filterNodeId != null)
        {
            connections.Add(new PropertyConnection()
            {
                InputPropertyName = StructureNode.FiltersPropertyName,
                OutputPropertyName = "Output",
                OutputNodeId = filterNodeId.Value
            });
        }

        nBuilder.WithConnections(connections.ToArray());
        lastId = id;
        return lastId;
    }

    private int? AddGroup(SvgGroup group, NodeGraphBuilder graph, StyleContext style, int? lastId,
        string connectionName = "Background")
    {
        int? childId = null;
        var connectTo = "Background";
        foreach (var child in group.Children)
        {
            StyleContext childStyle = style.WithElement(child);

            if (child is SvgPrimitive primitive)
            {
                childId = AddPrimitive(child, childStyle, graph, childId, connectTo);
            }
            else if (child is SvgGroup childGroup)
            {
                childId = AddGroup(childGroup, graph, childStyle, childId, connectTo);
            }
            else if (child is SvgImage image)
            {
                childId = AddImage(image, childStyle, graph, childId);
            }
            else if (child is SvgDocument nestedDocument)
            {
                childId = AddNestedDocument(nestedDocument, graph, childId, childStyle);
            }
        }

        NodeGraphBuilder.NodeBuilder nBuilder = graph.WithNodeOfType<FolderNode>(out int id)
            .WithName(group.Id.Unit != null ? group.Id.Unit.Value.Value : new LocalizedString("NEW_FOLDER"));

        int connectionsCount = 0;
        if (lastId != null) connectionsCount++;
        if (childId != null) connectionsCount++;

        PropertyConnection[] connections = new PropertyConnection[connectionsCount];
        if (lastId != null)
        {
            connections[0] = new PropertyConnection()
            {
                InputPropertyName = connectionName, OutputPropertyName = "Output", OutputNodeId = lastId.Value
            };
        }

        if (childId != null)
        {
            connections[^1] = new PropertyConnection()
            {
                InputPropertyName = "Content", OutputPropertyName = "Output", OutputNodeId = childId.Value
            };
        }

        if (connections.Length > 0)
        {
            nBuilder.WithConnections(connections);
        }

        lastId = id;

        return lastId;
    }

    private int? AddImage(SvgImage image, StyleContext style, NodeGraphBuilder graph, int? lastId)
    {
        byte[] bytes = TryReadImage(image.Href.Unit?.Value ?? "");

        Surface? imgSurface = bytes is { Length: > 0 } ? Surface.Load(bytes) : null;
        Surface? finalSurface = null;

        if (imgSurface != null)
        {
            if (imgSurface.Size.X != (int)image.Width.Unit?.PixelsValue ||
                imgSurface.Size.Y != (int)image.Height.Unit?.PixelsValue)
            {
                var resized = imgSurface.ResizeNearestNeighbor(
                    new VecI((int)image.Width.Unit?.PixelsValue, (int)image.Height.Unit?.PixelsValue));
                imgSurface.Dispose();
                imgSurface = resized;
            }
        }

        if (style.ViewboxSize.ShortestAxis > 0 && imgSurface != null)
        {
            finalSurface = new Surface((VecI)style.ViewboxSize);
            double x = image.X.Unit?.PixelsValue ?? 0;
            double y = image.Y.Unit?.PixelsValue ?? 0;
            finalSurface.DrawingSurface.Canvas.DrawSurface(imgSurface.DrawingSurface, (int)x, (int)y);
            imgSurface.Dispose();
        }

        if (finalSurface == null)
        {
            if (imgSurface != null)
            {
                finalSurface = imgSurface;
            }
            else
            {
                VecI size = new(
                    (int)(image.Width.Unit?.PixelsValue ?? 0),
                    (int)(image.Height.Unit?.PixelsValue ?? 0));
                if (size.ShortestAxis < 1)
                {
                    return lastId;
                }

                finalSurface = new Surface(size);
            }
        }

        var graphBuilder = graph.WithImageLayerNode(
            image.Id.Unit?.Value ?? new LocalizedString("NEW_LAYER").Value,
            finalSurface, ColorSpace.CreateSrgb(), out int id);

        if (lastId != null)
        {
            var nodeBuilder = graphBuilder.AllNodes[^1];

            Dictionary<string, object> inputValues = new()
            {
                { StructureNode.OpacityPropertyName, (float)(style.Opacity.Unit?.Value ?? 1f) }
            };

            nodeBuilder.WithInputValues(inputValues);
            nodeBuilder.WithConnections([
                new PropertyConnection()
                {
                    InputPropertyName = "Background", OutputPropertyName = "Output", OutputNodeId = lastId.Value
                }
            ]);
        }

        lastId = id;

        return lastId;
    }

    private int? AddNestedDocument(SvgDocument nestedDocument, NodeGraphBuilder graph, int? lastId, StyleContext style)
    {
        SvgDocumentBuilder nestedBuilder = new();

        var matrix = style.Transform.Unit?.MatrixValue ?? Matrix3X3.Identity;
        nestedDocument.Transform.Unit = new SvgTransformUnit(Matrix3X3.Identity);
        DocumentViewModel docViewModel = DocumentViewModel.Build(b =>
            nestedBuilder.Build(b, nestedDocument));

        var graphBuilder = graph.WithNodeOfType<NestedDocumentNode>(out int id)
            .WithName(nestedDocument.Id.Unit?.Value ?? new LocalizedString("NEW_LAYER"))
            .WithInputValues(new Dictionary<string, object>()
            {
                {
                    NestedDocumentNode.DocumentPropertyName,
                    new DocumentReference(null, docViewModel.Id, docViewModel.AccessInternalReadOnlyDocument())
                }
            }).WithAdditionalData(new Dictionary<string, object>() { { "TransformationMatrix", matrix } });

        if (lastId != null)
        {
            graphBuilder.WithConnections([
                new PropertyConnection()
                {
                    InputPropertyName = "Background", OutputPropertyName = "Output", OutputNodeId = lastId.Value
                }
            ]);
        }

        lastId = id;
        return lastId;
    }

    private Matrix3X3 TransformMatrix(
        Matrix3X3 matrix,
        SvgPreserveAspectRatioUnit stylePreserveAspectRatio,
        VecD size,
        VecD viewboxSize)
    {
        if (viewboxSize.X == 0 || viewboxSize.Y == 0)
        {
            return matrix;
        }

        float scaleX = (float)(size.X / viewboxSize.X);
        float scaleY = (float)(size.Y / viewboxSize.Y);

        float finalScaleX = scaleX;
        float finalScaleY = scaleY;

        float translateX = 0f;
        float translateY = 0f;

        if (stylePreserveAspectRatio.Align != SvgAspectRatio.None)
        {
            float uniformScale = stylePreserveAspectRatio.MeetOrSlice == SvgMeetOrSlice.Slice
                ? Math.Max(scaleX, scaleY)
                : Math.Min(scaleX, scaleY);

            finalScaleX = uniformScale;
            finalScaleY = uniformScale;

            float scaledWidth = (float)(viewboxSize.X * uniformScale);
            float scaledHeight = (float)(viewboxSize.Y * uniformScale);

            float remainingX = (float)size.X - scaledWidth;
            float remainingY = (float)size.Y - scaledHeight;

            switch (stylePreserveAspectRatio.Align)
            {
                case SvgAspectRatio.XMinYMin:
                    translateX = 0;
                    translateY = 0;
                    break;

                case SvgAspectRatio.XMidYMin:
                    translateX = remainingX / 2;
                    translateY = 0;
                    break;

                case SvgAspectRatio.XMaxYMin:
                    translateX = remainingX;
                    translateY = 0;
                    break;

                case SvgAspectRatio.XMinYMid:
                    translateX = 0;
                    translateY = remainingY / 2;
                    break;

                case SvgAspectRatio.XMidYMid:
                    translateX = remainingX / 2;
                    translateY = remainingY / 2;
                    break;

                case SvgAspectRatio.XMaxYMid:
                    translateX = remainingX;
                    translateY = remainingY / 2;
                    break;

                case SvgAspectRatio.XMinYMax:
                    translateX = 0;
                    translateY = remainingY;
                    break;

                case SvgAspectRatio.XMidYMax:
                    translateX = remainingX / 2;
                    translateY = remainingY;
                    break;

                case SvgAspectRatio.XMaxYMax:
                    translateX = remainingX;
                    translateY = remainingY;
                    break;
            }
        }

        matrix = matrix.PostConcat(Matrix3X3.CreateScaleTranslation(
            finalScaleX, finalScaleY,
            translateX, translateY));

        return matrix;
    }

    private byte[] TryReadImage(string svgHref)
    {
        if (string.IsNullOrEmpty(svgHref))
        {
            return [];
        }

        if (svgHref.StartsWith("data:image/png;base64,"))
        {
            return Convert.FromBase64String(svgHref.Replace("data:image/png;base64,", ""));
        }

        // TODO: Implement downloading images from the internet
        /*if (Uri.TryCreate(svgHref, UriKind.Absolute, out Uri? uri))
        {
            try
            {
                using WebClient client = new();
                return client.DownloadData(uri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return [];
            }
        }*/

        return [];
    }

    private EllipseVectorData AddEllipse(SvgElement element, StyleContext styleContext)
    {
        RectD viewBox = new RectD(styleContext.ViewboxOrigin, styleContext.ViewboxSize);
        if (element is SvgCircle circle)
        {
            return new EllipseVectorData(
                new VecD(circle.Cx.Unit?.ToPixels(viewBox) ?? 0, circle.Cy.Unit?.ToPixels(viewBox) ?? 0),
                new VecD(circle.R.Unit?.ToPixels(viewBox) ?? 0, circle.R.Unit?.ToPixels(viewBox) ?? 0));
        }

        if (element is SvgEllipse ellipse)
        {
            return new EllipseVectorData(
                new VecD(ellipse.Cx.Unit?.ToPixels(viewBox) ?? 0, ellipse.Cy.Unit?.ToPixels(viewBox) ?? 0),
                new VecD(ellipse.Rx.Unit?.ToPixels(viewBox) ?? 0, ellipse.Ry.Unit?.ToPixels(viewBox) ?? 0));
        }

        return null;
    }

    private LineVectorData AddLine(SvgLine element, StyleContext styleContext)
    {
        RectD viewBox = new RectD(styleContext.ViewboxOrigin, styleContext.ViewboxSize);
        return new LineVectorData(
            new VecD(element.X1.Unit?.ToPixels(viewBox) ?? 0, element.Y1.Unit?.ToPixels(viewBox) ?? 0),
            new VecD(element.X2.Unit?.ToPixels(viewBox) ?? 0, element.Y2.Unit?.ToPixels(viewBox) ?? 0));
    }

    private PathVectorData AddPath(SvgPath element, StyleContext styleContext)
    {
        VectorPath? path = null;
        if (element.PathData.Unit != null)
        {
            path = VectorPath.FromSvgPath(element.PathData.Unit.Value.Value);
        }

        if (element.FillRule.Unit != null)
        {
            path.FillType = element.FillRule.Unit.Value.Value switch
            {
                SvgFillRule.EvenOdd => PathFillType.EvenOdd,
                SvgFillRule.NonZero => PathFillType.Winding,
                _ => PathFillType.Winding
            };
        }

        StrokeCap strokeLineCap = StrokeCap.Butt;
        StrokeJoin strokeLineJoin = StrokeJoin.Miter;

        if (styleContext.StrokeLineCap.Unit != null)
        {
            strokeLineCap = (StrokeCap)(styleContext.StrokeLineCap.Unit?.Value ?? SvgStrokeLineCap.Butt);
        }

        if (styleContext.StrokeLineJoin.Unit != null)
        {
            strokeLineJoin = (StrokeJoin)(styleContext.StrokeLineJoin.Unit?.Value ?? SvgStrokeLineJoin.Miter);
        }

        return new PathVectorData(path) { StrokeLineCap = strokeLineCap, StrokeLineJoin = strokeLineJoin };
    }

    private RectangleVectorData AddRect(SvgRectangle element, StyleContext styleContext)
    {
        RectD viewBox = new RectD(styleContext.ViewboxOrigin, styleContext.ViewboxSize);
        double rx = element.Rx.Unit?.ToPixels(viewBox) ?? 0;
        double ry = element.Ry.Unit?.ToPixels(viewBox) ?? 0;
        double width = element.Width.Unit?.ToPixels(viewBox) ?? 0;
        double height = element.Height.Unit?.ToPixels(viewBox) ?? 0;

        double shortestAxis = Math.Min(width, height);

        double cornerRadius = Math.Max(rx, ry);
        double cornerRadiusPercent = cornerRadius / (shortestAxis / 2f);

        return new RectangleVectorData(
            element.X.Unit?.ToPixels(viewBox) ?? 0, element.Y.Unit?.ToPixels(viewBox) ?? 0,
            width, height) { CornerRadius = cornerRadiusPercent };
    }

    private TextVectorData AddText(SvgText element, StyleContext styleContext)
    {
        RectD viewBox = new RectD(styleContext.ViewboxOrigin, styleContext.ViewboxSize);
        Font font = styleContext.FontFamily.Unit.HasValue
            ? Font.FromFamilyName(styleContext.FontFamily.Unit.Value.Value)
            : Font.CreateDefault();
        FontFamilyName? missingFont = null;
        if (font == null)
        {
            font = Font.CreateDefault();
            missingFont = new FontFamilyName(styleContext.FontFamily.Unit.Value.Value);
        }

        font.Size = styleContext.FontSize.Unit?.ToPixels(viewBox) ?? 12;
        font.Bold = styleContext.FontWeight.Unit?.Value == SvgFontWeight.Bold;
        font.Italic = styleContext.FontStyle.Unit?.Value == SvgFontStyle.Italic;

        VecD position = new(
            element.X.Unit?.ToPixels(viewBox) ?? 0,
            element.Y.Unit?.ToPixels(viewBox) ?? 0);

        if (element.TextAnchor.Unit != null)
        {
            float anchorX = 0f;
            switch (element.TextAnchor.Unit.Value.Value)
            {
                case SvgTextAnchor.Start:
                    anchorX = 0f;
                    break;
                case SvgTextAnchor.Middle:
                    anchorX = -0.5f;
                    break;
                case SvgTextAnchor.End:
                    anchorX = -1f;
                    break;
            }

            font.MeasureText(element.Text.Unit.Value.Value, out RectD bounds);
            position.X += bounds.Width * anchorX;
        }

        return new TextVectorData(element.Text.Unit.Value.Value)
        {
            Position = position, Font = font, MissingFontFamily = missingFont, MissingFontText = "MISSING_FONT",
        };
    }

    private PathVectorData AddPoly(SvgElement element)
    {
        if (element is SvgPolyline polyline)
        {
            return new PathVectorData(VectorPath.FromPoints(polyline.GetPoints(), false));
        }

        if (element is SvgPolygon polygon)
        {
            return new PathVectorData(VectorPath.FromPoints(polygon.GetPoints(), true));
        }

        return null;
    }

    private void AddCommonShapeData(ShapeVectorData? shapeData, StyleContext styleContext)
    {
        if (shapeData == null)
        {
            return;
        }

        bool hasFill = styleContext.Fill.Unit?.Paintable is { AnythingVisible: true };
        bool hasStroke = styleContext.Stroke.Unit?.Paintable is { AnythingVisible: true } ||
                         styleContext.StrokeWidth.Unit is { PixelsValue: > 0 };
        bool hasTransform = styleContext.Transform.Unit is { MatrixValue.IsIdentity: false } ||
                            styleContext.PreserveAspectRatio.Unit != null;

        shapeData.Fill = hasFill;
        if (hasFill)
        {
            var target = styleContext.Fill.Unit;
            float opacity = (float)(styleContext.FillOpacity.Unit?.Value ?? 1);
            opacity = Math.Clamp(opacity, 0, 1);
            shapeData.FillPaintable = target?.Paintable ?? Colors.Black;
            shapeData.FillPaintable?.ApplyOpacity(opacity);
        }

        if (hasStroke)
        {
            var targetColor = styleContext.Stroke.Unit;
            var targetWidth = styleContext.StrokeWidth.Unit;

            float opacity = (float)(styleContext.StrokeOpacity.Unit?.Value ?? 1);
            opacity = Math.Clamp(opacity, 0, 1);
            shapeData.Stroke = targetColor?.Paintable ?? Colors.Transparent;
            shapeData.StrokeWidth = (float)(targetWidth?.PixelsValue ?? 1);
            shapeData.Stroke?.ApplyOpacity(opacity);
        }

        if (hasTransform)
        {
            var target = styleContext.Transform.Unit.HasValue
                ? styleContext.Transform.Unit
                : new SvgTransformUnit(Matrix3X3.Identity);
            shapeData.TransformationMatrix = TransformMatrix(target.Value.MatrixValue,
                styleContext.PreserveAspectRatio.Unit ??
                new SvgPreserveAspectRatioUnit(SvgAspectRatio.None, SvgMeetOrSlice.Meet),
                styleContext.DocumentSize,
                styleContext.ViewboxSize);
        }

        if (styleContext.ViewboxOrigin != VecD.Zero)
        {
            shapeData.TransformationMatrix = shapeData.TransformationMatrix.PostConcat(
                Matrix3X3.CreateTranslation((float)styleContext.ViewboxOrigin.X, (float)styleContext.ViewboxOrigin.Y));
        }
    }

    private Type MapImageFilterToNodeType(SvgFilterPrimitive? filter, out Dictionary<string, object> inputProperties)
    {
        inputProperties = new Dictionary<string, object>();

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        switch (filter)
        {
            case SvgFeDropShadow svgFeDropShadow:
                inputProperties.Add(ShadowNode.OffsetPropertyName, new VecD(
                    svgFeDropShadow.Dx.Unit?.PixelsValue ?? 0,
                    svgFeDropShadow.Dy.Unit?.PixelsValue ?? 0));
                inputProperties.Add(ShadowNode.SigmaPropertyName, new VecD(
                    svgFeDropShadow.StdDeviation.Unit?.PixelsValue ?? 0,
                    svgFeDropShadow.StdDeviation.Unit?.PixelsValue ?? 0));
                Color floodColor = svgFeDropShadow.FloodColor.Unit?.Color ?? Colors.Black;
                float floodOpacity = (float)(svgFeDropShadow.FloodOpacity.Unit?.NormalizedValue() ?? 1);
                floodColor = floodColor.WithAlpha((byte)(floodOpacity * 255));
                inputProperties.Add(ShadowNode.ColorPropertyName, floodColor);
                return typeof(ShadowNode);
            case SvgFeGaussianBlur svgFeGaussianBlur:
                inputProperties.Add(BlurNode.RadiusPropertyName, new VecD(
                    svgFeGaussianBlur.StdDeviation.Unit?.PixelsValue ?? 0,
                    svgFeGaussianBlur.StdDeviation.Unit?.PixelsValue ?? 0));
                return typeof(BlurNode);
            default:
                throw new ArgumentOutOfRangeException(nameof(filter));
        }

        return null;
    }
}
