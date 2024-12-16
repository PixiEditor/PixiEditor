using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Helpers;
using PixiEditor.SVG;
using PixiEditor.SVG.Elements;

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
                    if (element is SvgPrimitive primitive)
                    {
                        ShapeVectorData shapeData = null;
                        if (element is SvgEllipse or SvgCircle)
                        {
                            shapeData = AddEllipse(element);
                        }
                        /*else if (element is SvgLine line)
                        {
                            AddLine(graph, line);
                        }
                        else if (element is SvgPath pathElement)
                        {
                            AddPath(graph, pathElement);
                        }
                        else if (element is SvgRectangle rect)
                        {
                            AddRect(graph, rect);
                        }
                        else if (element is SvgGroup group)
                        {
                            AddGroup(graph, group);
                        }*/

                        AddCommonShapeData(primitive, shapeData);

                        graph.WithNodeOfType<VectorLayerNode>(out int id)
                            .WithAdditionalData(new Dictionary<string, object>() { { "ShapeData", shapeData } });
                        
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

    private void AddCommonShapeData(SvgPrimitive primitive, ShapeVectorData? shapeData)
    {
        if (shapeData == null)
        {
            return;
        }

        bool hasFill = primitive.Fill.Unit is { Color.A: > 0 };
        bool hasStroke = primitive.Stroke.Unit is { Color.A: > 0 };
        bool hasTransform = primitive.Transform.Unit is { MatrixValue.IsIdentity: false };

        if (hasFill)
        {
            shapeData.Fill = true;
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
