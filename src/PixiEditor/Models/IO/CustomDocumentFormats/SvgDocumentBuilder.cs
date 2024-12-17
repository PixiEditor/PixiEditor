using System.Diagnostics.CodeAnalysis;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Helpers;
using PixiEditor.Parser.Graph;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;
using PixiEditor.SVG.Enums;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Models.IO.CustomDocumentFormats;

internal class SvgDocumentBuilder : IDocumentBuilder
{
    public IReadOnlyCollection<string> Extensions { get; } = [".svg"];

    public void Build(DocumentViewModelBuilder builder, string path)
    {
        string xml = File.ReadAllText(path);
        SvgDocument document = SvgDocument.Parse(xml);

        StyleContext styleContext = new(document);

        builder.WithSize((int)document.ViewBox.Unit.Value.Value.Width, (int)document.ViewBox.Unit.Value.Value.Height)
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
                }

                graph.WithOutputNode(lastId, "Output");
            });
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
            shapeData = AddEllipse(element);
            name = VectorEllipseToolViewModel.NewLayerKey;
        }
        else if (element is SvgLine line)
        {
            shapeData = AddLine(line);
            name = VectorLineToolViewModel.NewLayerKey;
        }
        else if (element is SvgPath pathElement)
        {
            shapeData = AddPath(pathElement);
            name = VectorPathToolViewModel.NewLayerKey;
        }
        else if (element is SvgRectangle rect)
        {
            shapeData = AddRect(rect);
            name = VectorRectangleToolViewModel.NewLayerKey;
        }

        AddCommonShapeData(shapeData, styleContext);

        NodeGraphBuilder.NodeBuilder nBuilder = graph.WithNodeOfType<VectorLayerNode>(out int id)
            .WithName(name)
            .WithAdditionalData(new Dictionary<string, object>() { { "ShapeData", shapeData } });

        if (lastId != null)
        {
            nBuilder.WithConnections([
                new PropertyConnection()
                {
                    InputPropertyName = connectionName, OutputPropertyName = "Output", OutputNodeId = lastId.Value
                }
            ]);
        }

        lastId = id;
        return lastId;
    }

    private int? AddGroup(SvgGroup group, NodeGraphBuilder graph, StyleContext style, int? lastId, string connectionName = "Background")
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
        }

        NodeGraphBuilder.NodeBuilder nBuilder = graph.WithNodeOfType<FolderNode>(out int id)
            .WithName(group.Id.Unit != null ? group.Id.Unit.Value.Value : new LocalizedString("NEW_FOLDER"));

        if (lastId != null)
        {
            nBuilder.WithConnections([
                new PropertyConnection()
                {
                    InputPropertyName = connectionName, OutputPropertyName = "Output", OutputNodeId = lastId.Value
                },
                new PropertyConnection()
                {
                    InputPropertyName = "Content", OutputPropertyName = "Output", OutputNodeId = childId.Value
                }
            ]);
        }

        lastId = id;

        return lastId;
    }

    private EllipseVectorData AddEllipse(SvgElement element)
    {
        if (element is SvgCircle circle)
        {
            return new EllipseVectorData(
                new VecD(circle.Cx.Unit.Value.Value, circle.Cy.Unit.Value.Value),
                new VecD(circle.R.Unit.Value.Value, circle.R.Unit.Value.Value));
        }

        if (element is SvgEllipse ellipse)
        {
            return new EllipseVectorData(
                new VecD(ellipse.Cx.Unit.Value.Value, ellipse.Cy.Unit.Value.Value),
                new VecD(ellipse.Rx.Unit.Value.Value, ellipse.Ry.Unit.Value.Value));
        }

        return null;
    }

    private LineVectorData AddLine(SvgLine element)
    {
        return new LineVectorData(
            new VecD(element.X1.Unit.Value.Value, element.Y1.Unit.Value.Value),
            new VecD(element.X2.Unit.Value.Value, element.Y2.Unit.Value.Value));
    }

    private PathVectorData AddPath(SvgPath element)
    {
        VectorPath path = VectorPath.FromSvgPath(element.PathData.Unit.Value.Value);

        if (element.FillRule.Unit != null)
        {
            path.FillType = element.FillRule.Unit.Value.Value switch
            {
                SvgFillRule.EvenOdd => PathFillType.EvenOdd,
                SvgFillRule.NonZero => PathFillType.Winding,
                _ => PathFillType.Winding
            };
        }

        return new PathVectorData(path);
    }

    private RectangleVectorData AddRect(SvgRectangle element)
    {
        return new RectangleVectorData(element.X.Unit.Value.Value, element.Y.Unit.Value.Value,
            element.Width.Unit.Value.Value, element.Height.Unit.Value.Value);
    }

    private void AddCommonShapeData(ShapeVectorData? shapeData, StyleContext styleContext)
    {
        if (shapeData == null)
        {
            return;
        }

        bool hasFill = styleContext.Fill.Unit is { Color.A: > 0 };
        bool hasStroke = styleContext.Stroke.Unit is { Color.A: > 0 };
        bool hasTransform = styleContext.Transform.Unit is { MatrixValue.IsIdentity: false };

        shapeData.Fill = hasFill;
        if (hasFill)
        {
            var target = styleContext.Fill.Unit;
            shapeData.FillColor = target.Value.Color;
        }

        if (hasStroke)
        {
            var targetColor = styleContext.Stroke.Unit;
            var targetWidth = styleContext.StrokeWidth.Unit;
            
            shapeData.StrokeColor = targetColor.Value.Color;
            shapeData.StrokeWidth = (float)targetWidth.Value.Value;
        }

        if (hasTransform)
        {
            var target = styleContext.Transform.Unit;
            shapeData.TransformationMatrix = target.Value.MatrixValue;
        }
    }
}
