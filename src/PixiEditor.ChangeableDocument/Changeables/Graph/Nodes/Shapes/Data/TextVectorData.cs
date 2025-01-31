using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class TextVectorData : ShapeVectorData, IDisposable
{
    private string text;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            richText = new RichText(value);
        }
    }

    public VecD Position { get; set; }

    public double MaxWidth { get; set; } = double.MaxValue;
    public Font Font { get; set; } = Font.CreateDefault();
    public double? Spacing { get; set; }
    public bool AntiAlias { get; set; } = true;

    public override RectD GeometryAABB
    {
        get
        {
            RectD bounds = richText.MeasureBounds(Font);
            return bounds.Offset(Position);
        }
    }

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public override RectD VisualAABB => GeometryAABB;
    public VectorPath? Path { get; set; }
    public FontFamilyName? MissingFontFamily { get; set; }
    public string MissingFontText { get; set; }

    private RichText richText;

    public override VectorPath ToPath()
    {
        var path = Font.GetTextPath(Text);
        path.Offset(Position);

        return path;
    }

    public override void RasterizeGeometry(Canvas canvas)
    {
        Rasterize(canvas, false);
    }

    public override void RasterizeTransformed(Canvas canvas)
    {
        Rasterize(canvas, true);
    }

    private void Rasterize(Canvas canvas, bool applyTransform)
    {
        int num = 0;
        if (applyTransform)
        {
            num = canvas.Save();
            ApplyTransformTo(canvas);
        }

        using Paint paint = new Paint() { IsAntiAliased = AntiAlias };

        richText.Fill = Fill;
        richText.FillColor = FillColor;
        richText.StrokeColor = StrokeColor;
        richText.StrokeWidth = StrokeWidth;
        richText.Spacing = Spacing;

        if (MissingFontFamily != null)
        {
            paint.Color = Fill ? FillColor : StrokeColor;
            canvas.DrawText($"{MissingFontText}: " + MissingFontFamily.Value.Name, Position, Font, paint);
        }
        else
        {
            PaintText(canvas, paint);
        }

        if (applyTransform)
        {
            canvas.RestoreToCount(num);
        }
    }

    private void PaintText(Canvas canvas, Paint paint)
    {
        richText.Paint(canvas, Position, Font, paint, Path);
    }

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(Text);
    }

    public override int GetCacheHash()
    {
        return HashCode.Combine(Text, Position, Font, StrokeColor, FillColor, StrokeWidth, TransformationMatrix);
    }

    public override int CalculateHash()
    {
        return GetCacheHash();
    }

    public void Dispose()
    {
        Font.Dispose();
        Path?.Dispose();
    }
}
