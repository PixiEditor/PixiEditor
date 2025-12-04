using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Mesh;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Image;

[NodeInfo("Pattern")]
public class PatternNode : RenderNode
{
    public InputProperty<Texture?> Fill { get; }
    public InputProperty<Matrix3X3> FillMatrix { get; }
    public InputProperty<double> Spacing { get; }
    public InputProperty<ShapeVectorData?> Path { get; }
    public InputProperty<PatternAlignment> Alignment { get; }
    public InputProperty<PatternStretching> Stretching { get; }

    public PatternNode()
    {
        Fill = CreateInput<Texture>("Fill", "FILL", null);
        FillMatrix = CreateInput<Matrix3X3>("FillMatrix", "MATRIX", Matrix3X3.Identity);
        Spacing = CreateInput<double>("Spacing", "SPACING_LABEL", 0)
            .WithRules(x => x.Min(0d));
        Path = CreateInput<ShapeVectorData>("Path", "SHAPE", null);
        Alignment = CreateInput<PatternAlignment>("Alignment", "ALIGNMENT", PatternAlignment.Center);
        Stretching = CreateInput<PatternStretching>("Stretching", "STRETCHING", PatternStretching.StretchToFit);
    }

    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        float spacing = (float)Spacing.Value;

        if (Fill.Value == null || Path.Value == null)
            return;

        if (spacing == 0)
        {
            spacing = Fill.Value.Size.X;
        }

        float distance = 0;

        using var path = Path.Value.ToPath(true);
        if (path == null)
            return;

        using Paint tilePaint = new Paint();
        using var snapshot = Fill.Value.DrawingSurface.Snapshot();
        using var shader = snapshot.ToShader(TileMode.Clamp, TileMode.Clamp, FillMatrix.Value);
        tilePaint.Shader = shader;

        while (distance < path.Length)
        {
            if (Stretching.Value == PatternStretching.PlaceAlong)
            {
                PlaceAlongPath(surface, snapshot, path, distance);
            }
            else if (Stretching.Value == PatternStretching.StretchToFit)
            {
                PlaceStretchToFit(surface, path, distance, spacing, tilePaint);
            }


            distance += spacing;
        }
    }

    private void PlaceAlongPath(Canvas surface, Drawie.Backend.Core.Surfaces.ImageData.Image image,
        VectorPath path, float distance)
    {
        var matrix = path.GetMatrixAtDistance(distance, false, PathMeasureMatrixMode.GetPositionAndTangent);
        if (matrix == null)
            return;

        if (Alignment.Value == PatternAlignment.Center)
        {
            matrix = matrix.Concat(Matrix3X3.CreateTranslation(-Fill.Value.Size.X / 2f, -Fill.Value.Size.Y / 2f));
        }
        else if (Alignment.Value == PatternAlignment.Outside)
        {
            matrix = matrix.Concat(Matrix3X3.CreateTranslation(0, -Fill.Value.Size.Y));
        }

        surface.Save();
        surface.SetMatrix(surface.TotalMatrix.Concat(matrix));
        surface.DrawImage(image, 0, 0);
        surface.Restore();
    }

    private void PlaceStretchToFit(Canvas surface, VectorPath path, float distance, float spacing,
        Paint tilePaint)
    {
        int texWidth = Fill.Value.Size.X;
        int texHeight = Fill.Value.Size.Y;

        // Iterate over each column of the texture (1px wide quads)
        for (int x = 0; x < texWidth; x++)
        {
            float u0 = (float)x / texWidth;
            float u1 = (float)(x + 1) / texWidth;

            float d0 = distance + u0 * spacing;
            float d1 = distance + u1 * spacing;

            var startSegment = path.GetPositionAndTangentAtDistance(d0, false);
            var endSegment = path.GetPositionAndTangentAtDistance(d1, false);

            var startNormal = new VecD(-startSegment.W, startSegment.Z).Normalize();
            var endNormal = new VecD(-endSegment.W, endSegment.Z).Normalize();

            float halfHeight = texHeight / 2f;

            VecD start = new VecD(startSegment.X, startSegment.Y);
            VecD end = new VecD(endSegment.X, endSegment.Y);

            if (Alignment.Value == PatternAlignment.Inside)
            {
                start += startNormal * halfHeight;
                end += endNormal * halfHeight;
            }
            else if (Alignment.Value == PatternAlignment.Outside)
            {
                start -= startNormal * halfHeight;
                end -= endNormal * halfHeight;
            }

            var v0 = start - startNormal * halfHeight;
            var v1 = start + startNormal * halfHeight;
            var v2 = end + endNormal * halfHeight;
            var v3 = end - endNormal * halfHeight;

            var texCoords = new VecF[] { new(x, texHeight), new(x, 0), new(x + 1, 0), new(x + 1, texHeight) };

            var verts = new VecF[] { (VecF)v0, (VecF)v1, (VecF)v2, (VecF)v3 };
            var indices = new ushort[] { 0, 1, 2, 0, 2, 3 };
            Color[] colors = { Colors.Transparent, Colors.Transparent, Colors.Transparent, Colors.Transparent };

            using var vertices = new Vertices(VertexMode.Triangles, verts, texCoords, colors, indices);
            surface.DrawVertices(vertices, BlendMode.SrcOver, tilePaint);
        }
    }

    public override Node CreateCopy()
    {
        return new PatternNode();
    }
}

public enum PatternAlignment
{
    Center,
    Outside,
    Inside,
}

public enum PatternStretching
{
    PlaceAlong,
    StretchToFit,
}
