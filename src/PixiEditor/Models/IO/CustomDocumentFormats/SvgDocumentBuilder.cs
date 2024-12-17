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

        builder.WithSize((int)document.ViewBox.Width, (int)document.ViewBox.Height)
            .WithGraph(graph =>
            {
                int? lastId = null;
                foreach (SvgElement element in document.Children)
                {
                    LocalizedString name = "";
                    if (element is SvgPrimitive primitive)
                    {
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
                        /*else if (element is SvgRectangle rect)
                        {
                            AddRect(graph, rect);
                        }
                        else if (element is SvgGroup group)
                        {
                            AddGroup(graph, group);
                        }*/

                        AddCommonShapeData(primitive, shapeData);

                        NodeGraphBuilder.NodeBuilder nBuilder = graph.WithNodeOfType<VectorLayerNode>(out int id)
                            .WithName(name)
                            .WithAdditionalData(new Dictionary<string, object>() { { "ShapeData", shapeData } });

                        if (lastId != null)
                        {
                            nBuilder.WithConnections([
                                new PropertyConnection()
                                {
                                    InputPropertyName = "Background",
                                    OutputPropertyName = "Output",
                                    OutputNodeId = lastId.Value
                                }
                            ]);
                        }

                        lastId = id;
                    }
                }

                graph.WithOutputNode(lastId, "Output");
            });
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
        
        if(element.FillRule.Unit != null)
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

    private void AddCommonShapeData(SvgPrimitive primitive, ShapeVectorData? shapeData)
    {
        if (shapeData == null)
        {
            return;
        }

        bool hasFill = primitive.Fill.Unit is { Color.A: > 0 };
        bool hasStroke = primitive.Stroke.Unit is { Color.A: > 0 };
        bool hasTransform = primitive.Transform.Unit is { MatrixValue.IsIdentity: false };

        shapeData.Fill = hasFill;
        if (hasFill)
        {
            shapeData.FillColor = primitive.Fill.Unit.Value.Color;
        }

        if (hasStroke)
        {
            shapeData.StrokeColor = primitive.Stroke.Unit.Value.Color;
            shapeData.StrokeWidth = (float)primitive.StrokeWidth.Unit.Value.Value;
        }

        if (hasTransform)
        {
            shapeData.TransformationMatrix = primitive.Transform.Unit.Value.MatrixValue;
        }
    }
}
