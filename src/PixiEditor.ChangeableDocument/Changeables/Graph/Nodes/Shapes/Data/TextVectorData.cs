using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces.Shapes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

public class TextVectorData : ShapeVectorData, IReadOnlyTextData, IScalable
{
    private string text;
    private double? spacing = null;
    private double strokeWidth = 1;
    private FontData font;
    private VectorPath? path;

    public string Text
    {
        get => text;
        set
        {
            text = value;
        }
    }

    public VecD Position { get; set; }


    public double MaxWidth { get; set; } = double.MaxValue;

    public FontData Font
    {
        get => font;
        set
        {
            font = value;
        }
    }

    public bool Bold
    {
        get => font.Bold;
        set
        {
            font.Bold = value;
        }
    }

    public bool Italic
    {
        get => font.Italic;
        set
        {
            font.Italic = value;
        }
    }

    public Font ConstructFont()
    {
        return Font.ToFont();
    }

    double IReadOnlyTextData.Spacing => Spacing ?? Font.Size;

    public double? Spacing
    {
        get => spacing;
        set
        {
            spacing = value;
        }
    }

    public bool AntiAlias { get; set; } = true;

    public override RectD GeometryAABB
    {
        get
        {
            var richText = CreateRichText();
            using var nativeFont = ConstructFont();
            var bounds = richText.MeasureBounds(nativeFont);
            return bounds.Offset(Position);
        }
    }

    public override ShapeCorners TransformationCorners =>
        new ShapeCorners(GeometryAABB).WithMatrix(TransformationMatrix);

    public override RectD VisualAABB => GeometryAABB;

    public VectorPath? Path
    {
        get => path;
        set
        {
            path = value;
            // TODO: properly calculate bounds
            //lastBounds = richText.MeasureBounds(Font);
        }
    }

    public string MissingFontText { get; set; }
    public VecD PathOffset { get; set; }

    private double _spacing;

    public TextVectorData()
    {
    }

    public TextVectorData(string text)
    {
        Text = text;
    }


    public override VectorPath ToPath(bool transformed = false)
    {
        RichText richText = CreateRichText();
        using Font nativeFont = ConstructFont();
        var path = richText.ToPath(nativeFont);
        path.Offset(Position);

        if (transformed)
        {
            path.Transform(TransformationMatrix);
        }

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
        using var nativeFont = Font.ToFont();

        if (nativeFont == null)
        {
            paint.SetPaintable(Fill ? FillPaintable : Stroke);
            canvas.DrawText($"{MissingFontText}: " + Font.Family.Name, Position, nativeFont, paint);
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

    private RichText CreateRichText()
    {
        return new RichText(Text)
        {
            Fill = Fill,
            FillPaintable = FillPaintable,
            StrokePaintable = Stroke,
            StrokeWidth = StrokeWidth,
            Spacing = Spacing,
            MaxWidth = MaxWidth,
        };
    }

    private void PaintText(Canvas canvas, Paint paint)
    {
        using Font nativeFont = ConstructFont();
        CreateRichText().Paint(canvas, Position, nativeFont, paint, Path, PathOffset);
    }

    public override bool IsValid()
    {
        return !string.IsNullOrEmpty(Text);
    }

    /*protected override void AdjustCopy(ShapeVectorData copy)
    {
        if (copy is TextVectorData textData)
        {
            textData.Font = Font;
            textData.Text = Text;
            textData.Position = Position;
            textData.Spacing = Spacing;
            textData.AntiAlias = AntiAlias;
            textData.MissingFontFamily = MissingFontFamily;
            textData.MissingFontText = MissingFontText;
            textData.MaxWidth = MaxWidth;
        }
    }*/

    protected override int GetSpecificHash()
    {
        HashCode hash = new();
        hash.Add(Text);
        hash.Add(Position);
        hash.Add(Font);
        hash.Add(Spacing);
        hash.Add(AntiAlias);
        hash.Add(MissingFontText);
        hash.Add(MaxWidth);
        hash.Add(Bold);
        hash.Add(Italic);
        hash.Add(PathOffset);

        return hash.ToHashCode();
    }

    public void Resize(VecD multiplier)
    {
        // TODO: Resize font size
        /*Position = Position.Multiply(multiplier);
        if(Font != null)
        {
            Font.Size *= multiplier.Y;
        }

        if (Spacing.HasValue)
        {
            Spacing *= multiplier.Y;
        }*/

        TransformationMatrix =
            TransformationMatrix.PostConcat(Matrix3X3.CreateScale((float)multiplier.X, (float)multiplier.Y));
    }

    protected bool Equals(TextVectorData other)
    {
        return base.Equals(other) && Position.Equals(other.Position) && MaxWidth.Equals(other.MaxWidth) &&
               AntiAlias == other.AntiAlias &&
               MissingFontText == other.MissingFontText
               && Text == other.Text && Font.Equals(other.Font) && Spacing.Equals(other.Spacing) &&
               Path == other.Path && Bold == other.Bold && Italic == other.Italic
               && PathOffset.Equals(other.PathOffset);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((TextVectorData)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Position, MaxWidth, AntiAlias, MissingFontText,
            Font, HashCode.Combine(Text, Spacing, Path, PathOffset));
    }
}
