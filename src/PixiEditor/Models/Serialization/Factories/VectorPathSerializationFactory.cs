using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.PathOverlay;

namespace PixiEditor.Models.Serialization.Factories;

internal class VectorPathSerializationFactory : VectorShapeSerializationFactory<PathVectorData>
{
    public override string DeserializationId { get; } = "PixiEditor.PathData";

    protected override void AddSpecificData(ByteBuilder builder, PathVectorData original)
    {
        if (original.Path == null)
        {
            return;
        }

        EditableVectorPath path = new EditableVectorPath(original.Path);

        builder.AddInt((int)original.StrokeLineJoin);
        builder.AddInt((int)original.StrokeLineCap);

        builder.AddInt((int)path.Path.FillType);
        builder.AddInt(path.SubShapes.Count);

        foreach (var subShape in path.SubShapes)
        {
            SerializeSubShape(builder, subShape);
        }
    }

    protected override bool DeserializeVectorData(ByteExtractor extractor, Matrix3X3 matrix, Paintable strokePaintable,
        bool fill, Paintable fillPaintable,
        float strokeWidth, (string serializerName, string serializerVersion) serializerData,
        out PathVectorData original)
    {
        StrokeJoin join = StrokeJoin.Round;
        StrokeCap cap = StrokeCap.Round;

        if (!IsFilePreVersion(serializerData, new Version(2, 0, 0, 62)))
        {
            join = (StrokeJoin)extractor.GetInt();
            cap = (StrokeCap)extractor.GetInt();
        }

        VectorPath path;
        if (IsOldSerializer(serializerData))
        {
            string svgPath = extractor.GetStringLegacyDontUse();
            path = VectorPath.FromSvgPath(svgPath);
        }
        else
        {
            path = DeserializePath(extractor).ToVectorPath();
        }

        original = new PathVectorData(path)
        {
            Stroke = strokePaintable,
            FillPaintable = fillPaintable,
            StrokeWidth = strokeWidth,
            TransformationMatrix = matrix,
            Fill = fill,
            StrokeLineJoin = join,
            StrokeLineCap = cap
        };

        return true;
    }

    private void SerializeSubShape(ByteBuilder builder, SubShape subShape)
    {
        builder.AddInt(subShape.IsClosed ? 1 : 0);
        builder.AddInt(subShape.Points.Count);

        foreach (var point in subShape.Points)
        {
            builder.AddFloat(point.Position.X);
            builder.AddFloat(point.Position.Y);

            builder.AddInt(point.Verb.IsEmptyVerb() ? -1 : (int)point.Verb.VerbType);
            builder.AddFloat(point.Verb.From.X);
            builder.AddFloat(point.Verb.From.Y);
            
            builder.AddFloat(point.Verb.To.X);
            builder.AddFloat(point.Verb.To.Y);

            if (HasControlPoint1(point))
            {
                builder.AddFloat(point.Verb.ControlPoint1.Value.X);
                builder.AddFloat(point.Verb.ControlPoint1.Value.Y);
            }

            if (HasControlPoint2(point))
            {
                builder.AddFloat(point.Verb.ControlPoint2.Value.X);
                builder.AddFloat(point.Verb.ControlPoint2.Value.Y);
            }

            if (IsConic(point))
            {
                builder.AddFloat(point.Verb.ConicWeight);
            }
        }
    }
    
    private EditableVectorPath DeserializePath(ByteExtractor extractor)
    {
        PathFillType fillType = (PathFillType)extractor.GetInt();
        int subShapesCount = extractor.GetInt();
        List<SubShape> subShapes = new List<SubShape>();

        for (int i = 0; i < subShapesCount; i++)
        {
            SubShape subShape = DeserializeSubShape(extractor);
            subShapes.Add(subShape);
        }

        return new EditableVectorPath(subShapes, fillType);
    }
    
    private SubShape DeserializeSubShape(ByteExtractor extractor)
    {
        bool isClosed = extractor.GetInt() == 1;
        int pointsCount = extractor.GetInt();
        List<ShapePoint> points = new List<ShapePoint>();

        for (int i = 0; i < pointsCount; i++)
        {
            VecF position = new VecF(extractor.GetFloat(), extractor.GetFloat());
            PathVerb verbType = (PathVerb)extractor.GetInt();
            
            VecF from = new VecF(extractor.GetFloat(), extractor.GetFloat());
            VecF to = new VecF(extractor.GetFloat(), extractor.GetFloat());
            
            VecF? controlPoint1 = verbType is PathVerb.Cubic or PathVerb.Quad or PathVerb.Conic
                ? new VecF(extractor.GetFloat(), extractor.GetFloat())
                : null;
            VecF? controlPoint2 = verbType is PathVerb.Cubic or PathVerb.Quad
                ? new VecF(extractor.GetFloat(), extractor.GetFloat())
                : null;
            float conicWeight = verbType == PathVerb.Conic ? extractor.GetFloat() : 0;

            Verb verb = new Verb(verbType, from, to, controlPoint1, controlPoint2, conicWeight);
            
            points.Add(new ShapePoint(position, points.Count, verb));
        }

        return new SubShape(points, isClosed);
    }

    private bool HasControlPoint1(ShapePoint point)
    {
        return (point.Verb.VerbType is PathVerb.Conic or PathVerb.Cubic or PathVerb.Quad) &&
               point.Verb.ControlPoint1.HasValue;
    }

    private bool HasControlPoint2(ShapePoint point)
    {
        return (point.Verb.VerbType is PathVerb.Cubic or PathVerb.Quad) &&
               point.Verb.ControlPoint2.HasValue;
    }

    private bool IsConic(ShapePoint point)
    {
        return point.Verb.VerbType == PathVerb.Conic;
    }

    private bool IsOldSerializer((string serializerName, string serializerVersion) serializerData)
    {
        if (string.IsNullOrEmpty(serializerData.serializerName) ||
            string.IsNullOrEmpty(serializerData.serializerVersion))
        {
            return false;
        }

        if (Version.TryParse(serializerData.serializerVersion, out Version version))
        {
            return version is { Major: 2, Minor: 0, Build: 0, Revision: < 35 };
        }

        return false;
    }
}
