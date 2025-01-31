using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class TextVectorData : ShapeVectorData, IReadOnlyTextData
{
    private string text;
    private Font font = Font.CreateDefault();
    private double? spacing = null;
    private double strokeWidth = 1;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            richText = new RichText(value);
            lastBounds = richText.MeasureBounds(Font);
        }
    }

    public VecD Position { get; set; }

    public double MaxWidth { get; set; } = double.MaxValue;

    public Font Font
    {
        get => font;
        set
        {
            font = value;
            lastBounds = richText.MeasureBounds(value);
        }
    }

    public double? Spacing
    {
        get => spacing;
        set
        {
            spacing = value;
            richText.Spacing = value;
            lastBounds = richText.MeasureBounds(Font);
        }
    }
    
    public bool AntiAlias { get; set; } = true;

    protected override void OnMatrixChanged()
    {
        lastBounds = richText.MeasureBounds(Font);
    }

    protected override void OnStrokeWidthChanged()
    {
        lastBounds = richText.MeasureBounds(Font);
    }

    public override RectD GeometryAABB
    {
        get
        {
            return lastBounds.Offset(Position);
        }
    }

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public override RectD VisualAABB => GeometryAABB;
    public VectorPath? Path { get; set; }
    public FontFamilyName? MissingFontFamily { get; set; }
    public string MissingFontText { get; set; }

    private RichText richText;
    private RectD lastBounds;

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

    protected override void AdjustCopy(ShapeVectorData copy)
    {
        if (copy is TextVectorData textData)
        {
            textData.Font = Font.FromFontFamily(Font.Family);
            textData.Font.Size = Font.Size;
            textData.Font.Edging = Font.Edging;
            textData.Font.SubPixel = Font.SubPixel;
        }
    }

    public override int CalculateHash()
    {
        return GetCacheHash();
    }
}
